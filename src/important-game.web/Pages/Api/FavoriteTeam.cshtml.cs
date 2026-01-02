using important_game.infrastructure.Contexts.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages.Api
{
    [IgnoreAntiforgeryToken]
    public class FavoriteTeamModel(IUserService userService) : PageModel
    {
        private readonly IUserService _userService = userService;

        public async Task<IActionResult> OnPostAsync([FromBody] FavoriteTeamRequest request, CancellationToken cancellationToken = default)
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

                // Add favorite team
                await _userService.AddFavoriteTeamAsync(user.UserId, request.TeamId, cancellationToken);

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

        public async Task<IActionResult> OnDeleteAsync([FromBody] FavoriteTeamRequest request, CancellationToken cancellationToken = default)
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

                // Remove favorite team
                await _userService.RemoveFavoriteTeamAsync(user.UserId, request.TeamId, cancellationToken);

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

        public async Task<IActionResult> OnGetAsync([FromQuery] int teamId, CancellationToken cancellationToken = default)
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
                            isFavorite = await _userService.IsFavoriteTeamAsync(user.UserId, teamId, cancellationToken);
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
                Console.WriteLine($"Error in FavoriteTeam GET: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        public class FavoriteTeamRequest
        {
            public int TeamId { get; set; }
        }
    }
}
