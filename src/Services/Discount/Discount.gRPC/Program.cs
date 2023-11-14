using System.Reflection;
using Asp.Versioning;

using Discount.gRPC.Data;
using Discount.gRPC.Extensions;
using Discount.gRPC.Repositories;
using Discount.gRPC.Services;
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

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
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

builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
builder.Services.AddSingleton<DiscountMapper>();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "Postgres Health", failureStatus: HealthStatus.Unhealthy)
    .AddIdentityServer(new Uri(builder.Configuration["JWT:ValidIssuer"]!), name: "Duende IdentityServer Health", failureStatus: HealthStatus.Degraded)
    .AddElasticsearch(builder.Configuration["Serilog:WriteTo:1:Args:nodeUris"]!, "Elasticsearch Health", HealthStatus.Degraded, timeout: TimeSpan.FromSeconds(2));

builder.Services.AddGrpc()
    .AddJsonTranscoding();

builder.Services.AddGrpcReflection()
    .AddGrpcSwagger();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(o =>
{
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var filePath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    o.IncludeXmlComments(filePath);
    o.IncludeGrpcXmlComments(filePath, true);

    o.OperationFilter<SwaggerDefaultValues>();
});

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

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("DockerDevelopment"))
{
    IdentityModelEventSource.ShowPII = true;
    app.MapSwaggerMiddleware();

    app.MigrateDatabase<Program>();
}

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<DiscountServiceV1>()
    .RequireAuthorization("ApiScope");

app.MapGrpcService<DiscountServiceV2>();

app.MapGrpcReflectionService();
app.MapCustomHealthChecks();

app.Run();
