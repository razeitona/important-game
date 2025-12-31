using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages;

public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        await HttpContext.SignOutAsync();
        return LocalRedirect(returnUrl ?? "/");
    }
}
