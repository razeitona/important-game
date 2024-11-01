using important_game.infrastructure.ImportantMatch;
using important_game.web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class MatchesModel(IExcitmentMatchService _matchService) : PageModel
    {
        public ExcitmentMatchesResponse Matches { get; private set; } = new ExcitmentMatchesResponse();

        public async Task OnGet()
        {
            var allMatches = await _matchService.GetAllMatchesAsync();

            Matches.Matches = allMatches;

            Matches.Leagues = allMatches.GroupBy(c => c.League.Name).Select(c => c.First().League).OrderBy(c => c.Id).ToList();
        }
    }
}
