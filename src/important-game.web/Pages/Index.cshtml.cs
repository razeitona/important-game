using important_game.infrastructure.ImportantMatch;
using important_game.infrastructure.ImportantMatch.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace important_game.web.Pages
{
    public class IndexModel(ILogger<IndexModel> _logger, IExcitmentMatchService _matchService, IConfiguration configuration) : PageModel
    {
        public List<ExcitementMatchDto> TrendingMatches { get; set; }
        public List<ExcitementMatchDto> OtherMatches { get; set; }
        public async Task OnGet()
        {
            var allMatches = await _matchService.GetAllMatchesAsync();

            TrendingMatches = GetTrendingMatches(allMatches);
            OtherMatches = GetUpcomingMatches(allMatches);
        }

        /// <summary>
        /// Extract top 5 trending matches
        /// </summary>
        public List<ExcitementMatchDto> GetTrendingMatches(List<ExcitementMatchDto> allMatches)
        {
            DateTime now = DateTime.Now;
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Calendar calendar = cultureInfo.Calendar;
            CalendarWeekRule calendarWeekRule = cultureInfo.DateTimeFormat.CalendarWeekRule;

            int weekNumber = calendar.GetWeekOfYear(now, calendarWeekRule, DayOfWeek.Monday);

            var matchesOfWeek = allMatches.Where(c =>
                c.MatchDate > now &&
                calendar.GetWeekOfYear(c.MatchDate.Date, calendarWeekRule, DayOfWeek.Monday) == weekNumber
                && c.ExcitementScore >= 0.6d
            ).OrderByDescending(c => c.ExcitementScore).Take(5).ToList();

            var ids = matchesOfWeek.Select(c => c.Id).ToHashSet();

            allMatches.RemoveAll(m => ids.Contains(m.Id));

            return matchesOfWeek;
        }


        /// <summary>
        /// Extract all live games and top 5 upcoming games
        /// </summary>
        public List<ExcitementMatchDto> GetUpcomingMatches(List<ExcitementMatchDto> allMatches)
        {
            var upcomingMatches = allMatches.Where(c => c.IsLive).ToList();

            upcomingMatches.AddRange(allMatches
                .Where(c => !c.IsLive)
                .Take(10));

            return upcomingMatches;
        }
    }
}
