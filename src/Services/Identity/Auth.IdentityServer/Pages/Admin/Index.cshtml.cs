using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auth.IdentityServer.Pages.Admin;
[SecurityHeaders]
[Authorize]
public class IndexModel : PageModel
{
    public void OnGet()
    {

    }
}