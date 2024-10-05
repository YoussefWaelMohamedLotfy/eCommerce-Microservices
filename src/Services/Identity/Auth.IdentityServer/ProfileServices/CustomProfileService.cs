using System.Security.Claims;
using Auth.IdentityServer.Models;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;

namespace Auth.IdentityServer.ProfileServices;

public sealed class CustomProfileService : ProfileService<ApplicationUser>
{
    public CustomProfileService(UserManager<ApplicationUser> userManager,
                                IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
                                ILogger<ProfileService<ApplicationUser>> logger)
        : base(userManager, claimsFactory, logger)
    {
    }

    protected override async Task GetProfileDataAsync(ProfileDataRequestContext context, ApplicationUser user)
    {
        var principal = await GetUserClaimsAsync(user).ConfigureAwait(false);
        var id = principal.Identity as ClaimsIdentity;

        if (!string.IsNullOrEmpty(user.FavouriteColor))
        {
            id.AddClaim(new("favourite_color", user.FavouriteColor));
        }

        context.AddRequestedClaims(principal.Claims);
    }
}
