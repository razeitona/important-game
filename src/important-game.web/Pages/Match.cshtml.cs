using important_game.infrastructure.Contexts.Matches;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Models;
using important_game.infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages;

public class MatchModel(ILogger<MatchModel> _logger, IMatchService _matchService) : PageModel
{
    public MatchDetailViewModel? MatchInfo { get; private set; }

    public async Task OnGet([FromRoute] string slug)
    {
        // Try to parse as integer for backward compatibility with old URLs
        if (int.TryParse(slug, out int matchId))
        {
            var match = await _matchService.GetMatchByIdAsync(matchId);

            if (match == null)
                return;

            // Redirect to new slug-based URL for SEO
            var redirectHomeSlug = SlugHelper.GenerateSlug(match.HomeTeamName);
            var redirectAwaySlug = SlugHelper.GenerateSlug(match.AwayTeamName);
            var newSlug = $"{redirectHomeSlug}-vs-{redirectAwaySlug}";

            Response.Redirect($"/match/{newSlug}", permanent: true);
            return;
        }

        // Parse slug format: "home-team-vs-away-team"
        var parts = slug.Split("-vs-", StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
        {
            _logger.LogWarning("Invalid slug format: {Slug}", slug);
            return;
        }

        var homeSlug = parts[0];
        var awaySlug = parts[1];

        // Get match by team slugs
        var matchBySlug = await _matchService.GetMatchByTeamSlugsAsync(homeSlug, awaySlug);

        if (matchBySlug == null)
        {
            _logger.LogWarning("Match not found for slug: {Slug}", slug);
            return;
        }

        MatchInfo = matchBySlug;
    }

}
