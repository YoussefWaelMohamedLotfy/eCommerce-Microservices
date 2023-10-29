using Asp.Versioning.ApiExplorer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Utilites.Swagger;

public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        => this.provider = provider;

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
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
    }
}
