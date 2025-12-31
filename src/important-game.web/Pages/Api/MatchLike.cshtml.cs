using important_game.infrastructure.Contexts.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages.Api
{
    [IgnoreAntiforgeryToken]
    public class MatchLikeModel(IUserService userService) : PageModel
    {
        private readonly IUserService _userService = userService;

        public async Task<IActionResult> OnPostAsync([FromBody] LikeRequest request, CancellationToken cancellationToken = default)
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

                // Add or update vote (always type 1 for like)
                await _userService.AddOrUpdateVoteAsync(user.UserId, request.MatchId, voteType: 1, cancellationToken);

                // Get updated vote count
                var voteCount = await _userService.GetMatchVoteCountAsync(request.MatchId, cancellationToken);

                return new JsonResult(new
                {
                    success = true,
                    liked = true,
                    voteCount = voteCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnDeleteAsync([FromBody] LikeRequest request, CancellationToken cancellationToken = default)
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

                // Remove vote
                await _userService.RemoveVoteAsync(user.UserId, request.MatchId, cancellationToken);

                // Get updated vote count
                var voteCount = await _userService.GetMatchVoteCountAsync(request.MatchId, cancellationToken);

                return new JsonResult(new
                {
                    success = true,
                    liked = false,
                    voteCount = voteCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetAsync([FromQuery] int matchId, CancellationToken cancellationToken = default)
        {
            try
            {
                bool isLiked = false;

                // Check if user is authenticated
                if (User.Identity?.IsAuthenticated == true)
                {
                    var googleId = User.FindFirst("GoogleId")?.Value;
                    if (!string.IsNullOrEmpty(googleId))
                    {
                        var user = await _userService.GetUserByGoogleIdAsync(googleId, cancellationToken);
                        if (user != null)
                        {
                            var vote = await _userService.GetUserVoteAsync(user.UserId, matchId, cancellationToken);
                            isLiked = vote != null;
                        }
                    }
                }

                // Get vote count
                var voteCount = await _userService.GetMatchVoteCountAsync(matchId, cancellationToken);

                return new JsonResult(new
                {
                    success = true,
                    liked = isLiked,
                    voteCount = voteCount
                });
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                Console.WriteLine($"Error in MatchLike GET: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        public class LikeRequest
        {
            public int MatchId { get; set; }
        }
    }
}
