using important_game.infrastructure.Contexts.BroadcastChannels;
using important_game.infrastructure.Contexts.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages.Api
{
    [IgnoreAntiforgeryToken]
    public class FavoriteBroadcastChannelModel(IUserService userService, IBroadcastChannelService broadcastChannelService) : PageModel
    {
        private readonly IUserService _userService = userService;
        private readonly IBroadcastChannelService _broadcastChannelService = broadcastChannelService;

        public async Task<IActionResult> OnPostAsync([FromBody] FavoriteChannelRequest request, CancellationToken cancellationToken = default)
        {
            // Require authentication
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized();
            }

            try
            {
                // Get user ID from claims
                var googleId = User.FindFirst("GoogleId")?.Value;
                if (string.IsNullOrEmpty(googleId))
                {
                    return Unauthorized();
                }

                // Get or create user
                var user = await _userService.GetUserByGoogleIdAsync(googleId, cancellationToken);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                // Add favorite channel
                await _broadcastChannelService.AddUserFavoriteChannelAsync(user.UserId, request.ChannelId, cancellationToken);

                return new JsonResult(new
                {
                    success = true,
                    isFavorite = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnDeleteAsync([FromBody] FavoriteChannelRequest request, CancellationToken cancellationToken = default)
        {
            // Require authentication
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized();
            }

            try
            {
                // Get user ID from claims
                var googleId = User.FindFirst("GoogleId")?.Value;
                if (string.IsNullOrEmpty(googleId))
                {
                    return Unauthorized();
                }

                // Get user
                var user = await _userService.GetUserByGoogleIdAsync(googleId, cancellationToken);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                // Remove favorite channel
                await _broadcastChannelService.RemoveUserFavoriteChannelAsync(user.UserId, request.ChannelId, cancellationToken);

                return new JsonResult(new
                {
                    success = true,
                    isFavorite = false
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetAsync([FromQuery] int channelId, CancellationToken cancellationToken = default)
        {
            try
            {
                bool isFavorite = false;

                // Check if user is authenticated
                if (User.Identity?.IsAuthenticated == true)
                {
                    var googleId = User.FindFirst("GoogleId")?.Value;
                    if (!string.IsNullOrEmpty(googleId))
                    {
                        var user = await _userService.GetUserByGoogleIdAsync(googleId, cancellationToken);
                        if (user != null)
                        {
                            isFavorite = await _broadcastChannelService.IsFavoriteChannelAsync(user.UserId, channelId, cancellationToken);
                        }
                    }
                }

                return new JsonResult(new
                {
                    success = true,
                    isFavorite = isFavorite
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FavoriteBroadcastChannel GET: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        public class FavoriteChannelRequest
        {
            public int ChannelId { get; set; }
        }
    }
}
