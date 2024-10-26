using important_game.infrastructure.ImportantMatch;
using important_game.infrastructure.ImportantMatch.Live;
using important_game.infrastructure.ImportantMatch.Models;
using important_game.web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace important_game.web.Pages
{
    public class IndexModel(ILogger<IndexModel> _logger, IExcitmentMatchService _matchService
        , IExcitmentMatchLiveProcessor _liveProcessor) : PageModel
    {
        public ExcitmentMatchMainResponse Matches { get; private set; } = new ExcitmentMatchMainResponse();


        public async Task OnGet()
        {
            var allMatches = await _matchService.GetAllMatchesAsync();

            Matches.LiveGames = await GetLiveGames(allMatches);
            Matches.WeekMatches = GetWeekMatches(allMatches);
            Matches.UpcomingMatch = GetUpcomingMatches(allMatches);
        }

        private async Task<List<LiveExcitementMatch>> GetLiveGames(List<ExcitementMatch> allMatches)
        {
            var allGames = allMatches
                                .Where(c => c.MatchDate < DateTime.UtcNow && c.MatchDate > DateTime.UtcNow.AddMinutes(-110))
                                .OrderByDescending(c => c.ExcitementScore)
                                .Take(5)
                                .ToList();

            var liveGames = new List<LiveExcitementMatch>();
            foreach (var match in allGames)
            {
                var liveES = await _liveProcessor.ProcessLiveMatchData(match.Id);
                liveGames.Add(new LiveExcitementMatch
                {
                    Id = match.Id,
                    AwayTeam = match.AwayTeam,
                    ExcitementScore = match.ExcitementScore,
                    HeadToHead = match.HeadToHead,
                    HomeTeam = match.HomeTeam,
                    League = match.League,
                    LiveExcitementScore = liveES,
                    MatchDate = match.MatchDate
                });
                allMatches.Remove(match);
            }

            return liveGames;
        }

        public List<ExcitementMatch> GetWeekMatches(List<ExcitementMatch> allMatches)
        {
            DateTime now = DateTime.Now;
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Calendar calendar = cultureInfo.Calendar;
            CalendarWeekRule calendarWeekRule = cultureInfo.DateTimeFormat.CalendarWeekRule;

            int weekNumber = calendar.GetWeekOfYear(now, calendarWeekRule, DayOfWeek.Monday);

            var matchesOfWeek = allMatches.Where(c =>
                c.MatchDate > now &&
                calendar.GetWeekOfYear(c.MatchDate.Date, calendarWeekRule, DayOfWeek.Monday) == weekNumber
            ).OrderByDescending(c => c.ExcitementScore).Take(5).ToList();

            return matchesOfWeek;
        }

        public List<ExcitementMatch> GetUpcomingMatches(List<ExcitementMatch> allMatches)
        {
            var upcomingMatches = allMatches
                .Where(c => c.MatchDate > DateTime.UtcNow)
                .OrderBy(c => c.MatchDate)
                .Take(5).ToList();

            return upcomingMatches;
        }
    }
}
