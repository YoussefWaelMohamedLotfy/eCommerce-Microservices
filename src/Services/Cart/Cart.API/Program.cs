using Asp.Versioning;
using Cart.API;
using Cart.API.Repositories;
using Cart.API.Services;
using Discount.gRPC.Protos;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Shared.Utilites.HealthChecks;
using Shared.Utilites.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(Serilogger.Configure);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(o => o.Protocols = HttpProtocols.Http1AndHttp2AndHttp3);
    options.ConfigureHttpsDefaults(o => o.AllowAnyClientCertificate());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["JWT:ValidIssuer"];
        options.RequireHttpsMetadata = builder.Environment.IsProduction();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api1");
    });
});

builder.Services.AddScoped<ICartRepository, CartRepository>();

builder.Services.AddStackExchangeRedisCache(x =>
{
    x.InstanceName = "Cart.API-";
    x.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});

builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(o
    => o.Address = new Uri(builder.Configuration.GetConnectionString("DiscountGrpcUrl")!));

builder.Services.AddScoped<DiscountGrpcService>();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["EventBusSettings:RabbitMQHostAddress"], "/", h =>
        {
            h.Username(builder.Configuration["EventBusSettings:RabbitMQHostUsername"]);
            h.Password(builder.Configuration["EventBusSettings:RabbitMQHostPassword"]);
        });

        config.ConfigureEndpoints(context);
    });
});

builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("RedisConnection")!, "Redis Health", HealthStatus.Unhealthy, timeout: TimeSpan.FromSeconds(2))
    .AddRabbitMQ(builder.Configuration["EventBusSettings:RabbitMQHealthCheckAddress"]!, name: "RabbitMQ Health", failureStatus: HealthStatus.Degraded)
    .AddIdentityServer(new Uri(builder.Configuration["JWT:ValidIssuer"]!), name: "Duende IdentityServer Health", failureStatus: HealthStatus.Unhealthy)
    .AddElasticsearch(builder.Configuration["Serilog:WriteTo:1:Args:nodeUris"]!, "Elasticsearch Health", HealthStatus.Degraded, timeout: TimeSpan.FromSeconds(2));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(o => o.OperationFilter<SwaggerDefaultValues>());

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ApiVersionReader = ApiVersionReader.Combine(
            new QueryStringApiVersionReader(),
            new UrlSegmentApiVersionReader()
        );
    o.ReportApiVersions = true;
}).AddApiExplorer(o =>
{
    o.GroupNameFormat = "'v'VVV";
    o.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

app.MapEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("DockerDevelopment"))
{
    app.MapSwaggerMiddleware();
    IdentityModelEventSource.ShowPII = true;
}

app.UseAuthentication();
app.UseAuthorization();

app.MapCustomHealthChecks();

app.Run();
