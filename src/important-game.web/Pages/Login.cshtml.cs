using important_game.infrastructure.Contexts.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace important_game.web.Pages;

public class LoginModel(IUserService userService) : PageModel
{
    private readonly IUserService _userService = userService;

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        // If already authenticated, redirect
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(returnUrl ?? "/");
        }

        // First check if this is the callback from Google
        var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (authenticateResult?.Succeeded == true)
        {
            // This is the callback from Google - process the login
            return await ProcessGoogleLoginAsync(authenticateResult, returnUrl);
        }

        // This is the initial login request - redirect to Google
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Page("/Login", pageHandler: null, values: new { returnUrl }),
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> ProcessGoogleLoginAsync(AuthenticateResult authenticateResult, string? returnUrl)
    {
        var claims = authenticateResult.Principal!.Claims;

        // Extract Google user information
        var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var picture = claims.FirstOrDefault(c => c.Type == "picture")?.Value;

        if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
        {
            return RedirectToPage("/Error");
        }

        // Get or create user in database
        var user = await _userService.GetOrCreateUserFromGoogleAsync(googleId, email, name, picture);

        if (user == null)
        {
            return RedirectToPage("/Error");
        }

        // Create application claims
        var appClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name ?? user.Email),
            new("GoogleId", user.GoogleId),
            new("PreferredTimezone", user.PreferredTimezone)
        };

        if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
        {
            appClaims.Add(new Claim("picture", user.ProfilePictureUrl));
        }

        var identity = new ClaimsIdentity(appClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // Sign in with cookie authentication
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // Redirect to return URL or home
        // Remove any query parameters from returnUrl to ensure clean redirect
        var redirectUrl = returnUrl ?? "/";
        if (redirectUrl.Contains('?'))
        {
            redirectUrl = redirectUrl.Split('?')[0];
        }

        return LocalRedirect(redirectUrl);
    }
}
