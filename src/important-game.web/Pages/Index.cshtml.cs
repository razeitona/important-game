using important_game.infrastructure.Contexts.Matches;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class IndexModel(IMatchService _matchService) : PageModel
    {
        public List<MatchDto> LiveMatches { get; set; } = [];
        public List<MatchDto> TrendingMatches { get; set; } = [];
        public List<MatchDto> OtherMatches { get; set; } = [];

        public async Task OnGet()
        {

            var unfinishedMatches = await _matchService.GetAllUnfinishedMatchesAsync();
            unfinishedMatches = unfinishedMatches.OrderByDescending(c => c.ExcitmentScore).ToList();

            var now = DateTime.UtcNow;
            foreach (var match in unfinishedMatches)
            {
                if (match.MatchDateUTC < now.AddMinutes(-120))
                    continue;

                if (IsLiveMatch(match, now))
                {
                    if (LiveMatches.Count >= 6)
                        continue;

                    LiveMatches.Add(match);
                    continue;
                }
                else if (IsTrendingMatch(match, now) && TrendingMatches.Count < 5)
                {
                    TrendingMatches.Add(match);
                    continue;
                }
                else if (OtherMatches.Count < 10)
                {
                    OtherMatches.Add(match);
                }
            }

        }

        private static bool IsLiveMatch(MatchDto match, DateTime now)
        {
            return match.MatchDateUTC <= now && match.MatchDateUTC >= now.AddMinutes(-120);
        }

        private static bool IsTrendingMatch(MatchDto match, DateTime now)
        {
            return match.ExcitmentScore >= 0.6d && match.MatchDateUTC < now.AddDays(7);
        }
    }
}
