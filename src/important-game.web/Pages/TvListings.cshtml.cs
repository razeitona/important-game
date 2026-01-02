using important_game.infrastructure.Contexts.BroadcastChannels;
using important_game.infrastructure.Contexts.BroadcastChannels.Models;
using important_game.infrastructure.Contexts.Users;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class TvListingsModel : PageModel
    {
        private readonly IBroadcastChannelService _broadcastChannelService;
        private readonly IUserService _userService;

        public TvListingsModel(IBroadcastChannelService broadcastChannelService, IUserService userService)
        {
            _broadcastChannelService = broadcastChannelService;
            _userService = userService;
        }

        public TvListingsViewModel? TvListings { get; private set; }

        public async Task OnGetAsync(CancellationToken cancellationToken = default)
        {
            // Get date range: next 7 days from today
            var startDate = DateTime.UtcNow.Date;
            var endDate = startDate.AddDays(7);

            // Check if user is authenticated and get their ID
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var googleId = User.FindFirst("GoogleId")?.Value;
                if (!string.IsNullOrEmpty(googleId))
                {
                    var user = await _userService.GetUserByGoogleIdAsync(googleId, cancellationToken);
                    if (user != null)
                    {
                        userId = user.UserId;
                    }
                }
            }

            // Get TV listings (filtered by user's favorite channels if logged in)
            TvListings = await _broadcastChannelService.GetTvListingsAsync(startDate, endDate, userId, cancellationToken);
        }
    }
}
