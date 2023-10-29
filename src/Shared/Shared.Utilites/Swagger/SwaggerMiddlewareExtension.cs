using Microsoft.AspNetCore.Builder;

namespace Shared.Utilites.Swagger;

public static class SwaggerMiddlewareExtension
{
    public static WebApplication MapSwaggerMiddleware(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(o =>
        {
            var descriptions = app.DescribeApiVersions();

            foreach (var description in descriptions)
            {
                var url = $"/swagger/{description.GroupName}/swagger.json";
                o.SwaggerEndpoint(url, description.GroupName.ToUpperInvariant());
            }

            o.OAuthClientId("api-swagger");
            o.OAuthScopes("profile", "openid", "api1");
            o.OAuthUsePkce();
            o.EnablePersistAuthorization();
        });
    
        return app;
    }
}
