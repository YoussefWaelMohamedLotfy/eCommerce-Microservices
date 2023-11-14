using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;

namespace Auth.IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource()
            {
                Name = "email-verification",
                UserClaims = new List<string>
                {
                    JwtClaimTypes.Email,
                    JwtClaimTypes.EmailVerified
                }
            },
            new IdentityResource()
            {
                Name = "phone-verification",
                UserClaims = new List<string>
                {
                    JwtClaimTypes.PhoneNumber,
                    JwtClaimTypes.PhoneNumberVerified
                }
            },
            new IdentityResource("color", new [] { "favorite_color" })
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope(name: "api1", displayName: "MyAPI"),
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("API1", "First API")
            {
                Scopes = { "api1" },
            },
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client
            {
                ClientId = "client",

                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                // secret for authentication
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                // scopes that client has access to
                AllowedScopes = { "api1" }
            },
            // interactive ASP.NET Core Web App
            new Client
            {
                ClientId = "web",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
            
                // where to redirect to after login
                RedirectUris =
                {
                    "https://localhost:7206/signin-oidc",
                    "http://localhost:5079/signin-oidc"
                },

                // where to redirect to after logout
                PostLogoutRedirectUris =
                {
                    "https://localhost:5001/signout-callback-oidc",
                    "http://localhost:5002/signout-callback-oidc"
                },

                AllowOfflineAccess = true,
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "email-verification",
                    "phone-verification",
                    "api1",
                    "color"
                }
            },
            // JavaScript BFF client
            new Client
            {
                ClientId = "bff",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
    
                // where to redirect to after login
                RedirectUris = { "https://localhost:5003/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:5003/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "email-verification",
                    "phone-verification",
                    "api1",
                    "color"
                }
            },
            new Client
            {
                ClientId = "api-swagger",
                RequireClientSecret = false,
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RedirectUris =
                {
                    "https://localhost:7133/swagger/oauth2-redirect.html", // Ordering API
                    "https://localhost:7154/swagger/oauth2-redirect.html", // Cart API
                    "https://localhost:7109/swagger/oauth2-redirect.html", // Discount API
                    "https://localhost:7206/swagger/oauth2-redirect.html", // Catalog API
                },
                AllowedCorsOrigins =
                {
                    "https://localhost:7133", // Ordering API
                    "https://localhost:7154", // Cart API
                    "https://localhost:7109", // Discount API
                    "https://localhost:7206", // Catalog API
                },
                AllowOfflineAccess = true,
                AllowedScopes = 
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "api1"
                }
            },
        };
}
