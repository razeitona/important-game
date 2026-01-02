using important_game.infrastructure.Contexts.BroadcastChannels;
using important_game.infrastructure.Contexts.BroadcastChannels.Models;
using important_game.infrastructure.Contexts.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages;

[Authorize]
public class SettingsModel(
    ILogger<SettingsModel> logger,
    IUserService userService,
    IBroadcastChannelService broadcastChannelService) : PageModel
{
    private readonly ILogger<SettingsModel> _logger = logger;
    private readonly IUserService _userService = userService;
    private readonly IBroadcastChannelService _broadcastChannelService = broadcastChannelService;

    public Dictionary<string, List<BroadcastChannelViewModel>> ChannelsByCountry { get; private set; } = new();
    public List<int> UserFavoriteChannelIds { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var userId = _userService.GetUserId(User);

        if (userId.HasValue)
        {
            ChannelsByCountry = await _broadcastChannelService.GetChannelsGroupedByCountryAsync();
            UserFavoriteChannelIds = await _broadcastChannelService.GetUserFavoriteChannelIdsAsync(userId.Value);
        }
    }

    public async Task<IActionResult> OnPostDeleteAccountAsync()
    {
        var userId = _userService.GetUserId(User);

        if (!userId.HasValue)
        {
            return RedirectToPage("/Login");
        }

        try
        {
            await _userService.DeleteUserAccountAsync(userId.Value);
            _logger.LogInformation("User account deleted: {UserId}", userId.Value);

            // Sign out the user
            await HttpContext.SignOutAsync();

            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user account: {UserId}", userId.Value);
            TempData["Error"] = "An error occurred while deleting your account. Please try again.";
            return RedirectToPage();
        }
    }
}
