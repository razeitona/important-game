using important_game.infrastructure.Contexts.Matches;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace important_game.web.Pages
{
    public class IndexModel(IMatchesService _matchService) : PageModel
    {
        public List<MatchDto> TrendingMatches { get; set; }
        public List<MatchDto> OtherMatches { get; set; }
        public async Task OnGet()
        {
            var allMatches = await _matchService.GetAllUpcomingMatchesAsync();

            TrendingMatches = GetTrendingMatches(allMatches);
            OtherMatches = GetUpcomingMatches(allMatches);
        }

        /// <summary>
        /// Extract top 5 trending matches
        /// </summary>
        public List<MatchDto> GetTrendingMatches(List<MatchDto> allMatches)
        {
            DateTime now = DateTime.UtcNow;
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Calendar calendar = cultureInfo.Calendar;
            CalendarWeekRule calendarWeekRule = cultureInfo.DateTimeFormat.CalendarWeekRule;

            int weekNumber = calendar.GetWeekOfYear(now, calendarWeekRule, DayOfWeek.Monday);

            var matchesOfWeek = allMatches.Where(c =>
                c.MatchDateUTC > now &&
                calendar.GetWeekOfYear(c.MatchDateUTC.Date, calendarWeekRule, DayOfWeek.Monday) == weekNumber
                && c.ExcitmentScore >= 0.6d
            ).OrderByDescending(c => c.ExcitmentScore).Take(5).ToList();

            var ids = matchesOfWeek.Select(c => c.MatchId).ToHashSet();

            allMatches.RemoveAll(m => ids.Contains(m.MatchId));

            return matchesOfWeek;
        }


        /// <summary>
        /// Extract all live games and top 5 upcoming games
        /// </summary>
        public List<MatchDto> GetUpcomingMatches(List<MatchDto> allMatches)
        {
            var upcomingMatches = allMatches.ToList();
            upcomingMatches.AddRange(allMatches.Take(10));
            return upcomingMatches;
        }
    }
}
