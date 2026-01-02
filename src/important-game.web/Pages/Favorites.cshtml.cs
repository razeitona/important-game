using important_game.infrastructure.Contexts.Matches;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Users;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages;

public class FavoritesModel(IUserService userService, IMatchService matchService) : PageModel
{
    private readonly IUserService _userService = userService;
    private readonly IMatchService _matchService = matchService;

    public List<MatchDto> Matches { get; set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        // Check if user is authenticated
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return;
        }

        try
        {
            // Get user's Google ID from claims
            var googleId = User.FindFirst("GoogleId")?.Value;
            if (string.IsNullOrEmpty(googleId))
            {
                return;
            }

            // Get user
            var user = await _userService.GetUserByGoogleIdAsync(googleId, cancellationToken);
            if (user == null)
            {
                return;
            }

            // Get all upcoming matches
            var userFavoriteMatches = await _matchService.GetUserFavoriteUpcomingMatchesAsync(user.UserId, cancellationToken);

            // Filter matches that include favorite teams
            var now = DateTime.UtcNow;
            Matches = userFavoriteMatches
                .Where(m => m.MatchDateUTC > now.AddMinutes(-120))
                .OrderBy(m => m.MatchDateUTC)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading favorites: {ex}");
        }
    }
}
