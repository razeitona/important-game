using important_game.infrastructure.ImportantMatch;
using important_game.web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class LiveModel(IExcitmentMatchService _matchService) : PageModel
    {
        public ExcitmentMatchLiveResponse Matches { get; private set; } = new ExcitmentMatchLiveResponse();

        public async Task OnGet()
        {
            var liveGames = (await _matchService.GetLiveMatchesAsync()).OrderBy(c => c.MatchDate);

            Matches.Matches.AddRange(liveGames);


        }
    }
}
