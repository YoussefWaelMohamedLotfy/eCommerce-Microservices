using System.Reflection;
using Discount.gRPC.Data;
using Discount.gRPC.Extensions;
using Discount.gRPC.Repositories;
using Discount.gRPC.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
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
        options.Authority = "https://localhost:5001";

        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

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
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "Postgres Health", failureStatus: HealthStatus.Degraded)
    .AddIdentityServer(new Uri(builder.Configuration["JWT:ValidIssuer"]!), name: "Duende IdentityServer Health", failureStatus: HealthStatus.Degraded);

builder.Services.AddGrpc()
    .AddJsonTranscoding();

builder.Services.AddGrpcReflection()
    .AddGrpcSwagger();

builder.Services.AddSwaggerGen(c =>
{
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var filePath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(filePath);
    c.IncludeGrpcXmlComments(filePath, true);
});
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MigrateDatabase<Program>();
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<DiscountService>()
    .RequireAuthorization("ApiScope");

app.MapGrpcReflectionService();

app.MapGet("/", () => "Communication with server is available as gRPC endpoints and REST API.");
app.MapCustomHealthChecks();

app.Run();
