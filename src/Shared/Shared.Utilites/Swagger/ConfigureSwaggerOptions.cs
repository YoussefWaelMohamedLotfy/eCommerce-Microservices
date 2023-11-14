using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Utilites.Swagger;

public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    private readonly IConfiguration _configuration;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IConfiguration configuration)
    {
        _provider = provider;
        _configuration = configuration;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new()
            {
                Title = $"Version {description.ApiVersion} API",
                Description = $"API V{description.ApiVersion}",
                Version = $"v{description.ApiVersion}",
            });
        }

        options.AddSecurityDefinition("Bearer", new()
        {
            In = ParameterLocation.Header,
            Description = "Please provide a valid token",
            Name = "JWT Bearer Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer",
        });

        options.AddSecurityDefinition("OAuth", new()
        {
            In = ParameterLocation.Header,
            Name = "OAuth PKCE Authorization",
            Description = "You'll be redirected to log in",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(_configuration["JWT:AuthURL"]!),
                    TokenUrl = new Uri(_configuration["JWT:TokenURL"]!)
                }
            },
            Type = SecuritySchemeType.OAuth2
        });

        options.AddSecurityRequirement(new()
        {
            {
                new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference()
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme,
                    }
                },
                Array.Empty<string>()
            }
        });

        options.AddSecurityRequirement(new()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Id = "OAuth", Type = ReferenceType.SecurityScheme }
                },
                Array.Empty<string>()
            }
        });
    }
}
