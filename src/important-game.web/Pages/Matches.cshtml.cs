using important_game.infrastructure.Contexts.Matches;
using important_game.infrastructure.Contexts.Matches.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class MatchesModel(IMatchService matchService) : PageModel
    {
        public MatchesViewModel Matches { get; private set; } = new MatchesViewModel();

        public async Task OnGet()
        {
            var allMatches = await matchService.GetAllMatchesAsync();
            Matches = allMatches;
        }
    }
}
