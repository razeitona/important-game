using important_game.infrastructure.Contexts.Matches;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class CalendarModel(IMatchService matchService) : PageModel
    {
        public int Year { get; private set; }
        public int Month { get; private set; }
        public List<MatchDto> Matches { get; private set; } = [];
        public Dictionary<int, List<MatchDto>> MatchesByDay { get; private set; } = [];
        public int DaysInMonth { get; private set; }
        public int FirstDayOfWeek { get; private set; }
        public string MonthName { get; private set; } = string.Empty;

        public async Task OnGetAsync([FromQuery] int? year, [FromQuery] int? month)
        {
            // Default to current month if not specified
            var now = DateTime.UtcNow;
            Year = year ?? now.Year;
            Month = month ?? now.Month;

            // Validate month/year
            if (Month < 1 || Month > 12)
            {
                Month = now.Month;
            }

            // Get month info
            var firstDayOfMonth = new DateTime(Year, Month, 1);
            DaysInMonth = DateTime.DaysInMonth(Year, Month);
            FirstDayOfWeek = (int)firstDayOfMonth.DayOfWeek; // 0 = Sunday, 1 = Monday, etc.
            MonthName = firstDayOfMonth.ToString("MMMM yyyy");

            // Get all matches for the month
            var allMatches = await matchService.GetAllMatchesAsync();

            // Filter matches for this month
            var startOfMonth = new DateTimeOffset(Year, Month, 1, 0, 0, 0, TimeSpan.Zero);
            var endOfMonth = startOfMonth.AddMonths(1);

            Matches = allMatches.Matches
                .Where(m => m.MatchDateUTC >= startOfMonth && m.MatchDateUTC < endOfMonth)
                .OrderBy(m => m.MatchDateUTC)
                .ToList();

            // Group matches by day
            MatchesByDay = Matches
                .GroupBy(m => m.MatchDateUTC.Day)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        public MatchDto? GetHighestExcitementMatch(int day)
        {
            if (!MatchesByDay.ContainsKey(day))
                return null;

            return MatchesByDay[day]
                .OrderByDescending(m => m.ExcitmentScore)
                .FirstOrDefault();
        }

        public int GetMatchCount(int day)
        {
            return MatchesByDay.ContainsKey(day) ? MatchesByDay[day].Count : 0;
        }

        public List<MatchDto> GetTop3Matches(int day)
        {
            if (MatchesByDay.TryGetValue(day, out var matches))
            {
                return matches
                    .OrderByDescending(m => m.ExcitmentScore)
                    .Take(3)
                    .ToList();
            }
            return [];
        }

        public double GetAverageExcitementScore(int day)
        {
            if (MatchesByDay.TryGetValue(day, out var matches) && matches.Any())
            {
                return matches.Average(m => m.ExcitmentScore);
            }
            return 0;
        }

        public string GetESColorClass(double esScore)
        {
            if (esScore < 0.25) return "es-low";
            if (esScore < 0.50) return "es-medium";
            if (esScore < 0.70) return "es-good";
            return "es-high";
        }
    }
}
