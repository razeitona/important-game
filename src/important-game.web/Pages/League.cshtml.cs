using important_game.infrastructure.ImportantMatch;
using important_game.web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class LeagueModel(ILogger<LeagueModel> _logger, IExcitmentMatchService _matchService) : PageModel
    {
        public ExcitmentMatchLeagueResponse Matches { get; private set; } = new ExcitmentMatchLeagueResponse();


        public async Task OnGet()
        {
            var allMatches = await _matchService.GetAllMatchesAsync();

            Matches.Leagues = allMatches.OrderByDescending(c => c.League.Id) //Ranking
                .GroupBy(c => c.League.Id)
                .ToDictionary(c => c.FirstOrDefault().League, v => v.OrderBy(c => c.MatchDate).ToList())
                .OrderByDescending(c => c.Key.Id) // Ranking
                .ToDictionary(c => c.Key, v => v.Value);

        }
    }
}
