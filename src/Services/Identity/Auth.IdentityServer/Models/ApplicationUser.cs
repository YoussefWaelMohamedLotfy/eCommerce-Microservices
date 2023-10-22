using Microsoft.AspNetCore.Identity;

namespace Auth.IdentityServer.Models;

public sealed class ApplicationUser : IdentityUser
{
    public string FavouriteColor { get; set; }
}
