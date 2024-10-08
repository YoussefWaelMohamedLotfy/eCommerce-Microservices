using Asp.Versioning;
using Catalog.API;
using Discount.gRPC.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Ordering.API;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;
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
            ValidateLifetime = true,
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

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EventBusConsumer>();

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
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "SQL Server Health", failureStatus: HealthStatus.Unhealthy)
    .AddRabbitMQ(builder.Configuration["EventBusSettings:RabbitMQHealthCheckAddress"]!, name: "RabbitMQ Health", failureStatus: HealthStatus.Degraded)
    .AddDbContextCheck<OrderingDbContext>("EF Core Health", HealthStatus.Unhealthy)
    .AddIdentityServer(new Uri(builder.Configuration["JWT:ValidIssuer"]!), name: "Duende IdentityServer Health", failureStatus: HealthStatus.Unhealthy)
    .AddElasticsearch(builder.Configuration["Serilog:WriteTo:1:Args:nodeUris"]!, "Elasticsearch Health", HealthStatus.Degraded, timeout: TimeSpan.FromSeconds(2));

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

    app.MigrateDatabase<OrderingDbContext>();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapCustomHealthChecks();

app.Run();
