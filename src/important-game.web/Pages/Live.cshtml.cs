using important_game.infrastructure.ImportantMatch;
using important_game.infrastructure.ImportantMatch.Live;
using important_game.infrastructure.ImportantMatch.Models;
using important_game.web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class LiveModel(ILogger<CalendarModel> _logger, IExcitmentMatchService _matchService
        , IExcitmentMatchLiveProcessor _liveProcessor) : PageModel
    {
        public ExcitmentMatchLiveResponse Matches { get; private set; } = new ExcitmentMatchLiveResponse();

        public async Task OnGet()
        {
            var allMatches = await _matchService.GetAllMatchesAsync();

            var liveGames = allMatches
                              .Where(c => c.MatchDate < DateTime.UtcNow && c.MatchDate > DateTime.UtcNow.AddMinutes(-110))
                              .OrderByDescending(c => c.ExcitementScore)
                              .ToList();

            foreach (var match in liveGames)
            {
                var liveES = await _liveProcessor.ProcessLiveMatchData(match.Id);
                Matches.Matches.Add(new LiveExcitementMatch
                {
                    Id = match.Id,
                    AwayTeam = match.AwayTeam,
                    ExcitementScore = match.ExcitementScore,
                    HeadToHead = match.HeadToHead,
                    HomeTeam = match.HomeTeam,
                    League = match.League,
                    LiveExcitementScore = liveES,
                    MatchDate = match.MatchDate,
                    Score = match.Score
                });
            }

        }
    }
}
