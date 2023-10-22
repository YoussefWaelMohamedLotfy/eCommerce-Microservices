using System.Security.Claims;
using Auth.IdentityServer.Data;
using Auth.IdentityServer.Models;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Auth.IdentityServer;

public static class SeedData
{
    public static async void EnsureSeedData(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        //scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.EnsureDeleted();
        scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

        var configcontext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        //configcontext.Database.EnsureDeleted();
        configcontext.Database.Migrate();

        var appContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
        //appContext.Database.EnsureDeleted();
        appContext.Database.Migrate();

        if (!configcontext.Clients.Any())
        {
            foreach (var client in Config.Clients)
            {
                configcontext.Clients.Add(client.ToEntity());
            }
            configcontext.SaveChanges();
        }

        if (!configcontext.IdentityResources.Any())
        {
            foreach (var resource in Config.IdentityResources)
            {
                configcontext.IdentityResources.Add(resource.ToEntity());
            }
            configcontext.SaveChanges();
        }

        if (!configcontext.ApiScopes.Any())
        {
            foreach (var resource in Config.ApiScopes)
            {
                configcontext.ApiScopes.Add(resource.ToEntity());
            }
            configcontext.SaveChanges();
        }

        if (!configcontext.ApiResources.Any())
        {
            foreach (var resource in Config.ApiResources)
            {
                configcontext.ApiResources.Add(resource.ToEntity());
            }
            configcontext.SaveChanges();
        }

        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var alice = await userMgr.FindByNameAsync("alice").ConfigureAwait(false);

        if (alice is null)
        {
            alice = new ApplicationUser
            {
                Id = "1",
                UserName = "alice",
                Email = "AliceSmith@email.com",
                EmailConfirmed = true,
                PhoneNumber = "01234",
                PhoneNumberConfirmed = true,
                FavouriteColor = "Red"
            };
            var result = await userMgr.CreateAsync(alice, "Pass123$").ConfigureAwait(false);

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = await userMgr.AddClaimsAsync(alice, new Claim[] {
                            new Claim(JwtClaimTypes.Name, "Alice Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Alice"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                        }).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            Log.Debug("alice created");
        }
        else
        {
            Log.Debug("alice already exists");
        }

        var bob = await userMgr.FindByNameAsync("bob").ConfigureAwait(false);

        if (bob is null)
        {
            bob = new ApplicationUser
            {
                Id = "2",
                UserName = "bob",
                Email = "BobSmith@email.com",
                EmailConfirmed = false,
                PhoneNumber = "578910",
                PhoneNumberConfirmed = false,
                FavouriteColor = "Blue"
            };
            var result = await userMgr.CreateAsync(bob, "Pass123$").ConfigureAwait(false);

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = await userMgr.AddClaimsAsync(bob, new Claim[] {
                            new Claim(JwtClaimTypes.Name, "Bob Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Bob"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                            new Claim("location", "somewhere")
                        }).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            Log.Debug("bob created");
        }
        else
        {
            Log.Debug("bob already exists");
        }
    }
}
