using important_game.infrastructure.Contexts.Matches;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages;

public class MatchModel(ILogger<MatchModel> _logger, IMatchService _matchService) : PageModel
{
    public MatchDetailViewModel? MatchInfo { get; private set; }

    public async Task OnGet([FromRoute] int id)
    {
        var match = await _matchService.GetMatchByIdAsync(id);

        if (match == null)
            return;

        MatchInfo = match;
    }

}
