using important_game.ui.Core.Models;
using important_game.ui.Infrastructure.ImportantMatch;
using important_game.web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace important_game.web.Pages
{
    public class MatchCardIndex : PageModel
    {
        private readonly ILogger<MatchCardIndex> _logger;
        private readonly IExcitmentMatchProcessor _excitmentMatchProcessor;
        public ExcitmentMatchResponse Matches { get; private set; } = new ExcitmentMatchResponse();

        public MatchCardIndex(ILogger<MatchCardIndex> logger, IExcitmentMatchProcessor excitmentMatchProcessor)
        {
            _logger = logger;
            _excitmentMatchProcessor = excitmentMatchProcessor;
        }


        public async Task OnGet()
        {

            List<ExcitementMatch> excitementMatches = null;
            if (System.IO.File.Exists("data.json"))
            {
                var rawData = await System.IO.File.ReadAllTextAsync("data.json");
                if (!string.IsNullOrWhiteSpace(rawData))
                {
                    excitementMatches = JsonSerializer.Deserialize<List<ExcitementMatch>>(rawData);
                }
            }

            if (excitementMatches == null)
            {
                excitementMatches = await _excitmentMatchProcessor.GetUpcomingExcitementMatchesAsync(new MatchImportanceOptions());

                if (excitementMatches == null)
                    return;
                else
                    await System.IO.File.WriteAllTextAsync("data.json", JsonSerializer.Serialize(excitementMatches));
            }


            var allMatches = excitementMatches!.OrderBy(c => c.MatchDate.Date >= DateTime.UtcNow.Date).OrderBy(c => c.MatchDate).ToList();

            Matches.Leagues = PrepareLeagues(allMatches);

            SetLiveGames(allMatches);
            SetTodaysBestMatch(allMatches);

            Matches.UpcomingMatch = allMatches.Where(c => c.MatchDate > DateTime.UtcNow).OrderByDescending(c => c.ExcitementScore).ToList();


        }

        private void SetLiveGames(List<ExcitementMatch> allMatches)
        {
            Matches.LiveGames = allMatches?
             .Where(c => c.MatchDate < DateTime.UtcNow && c.MatchDate > DateTime.UtcNow.AddMinutes(-110))
             .OrderByDescending(c => c.ExcitementScore).ToList();

            allMatches.RemoveAll(c => Matches.LiveGames.Contains(c));
        }

        private void SetTodaysBestMatch(List<ExcitementMatch>? allMatches)
        {
            var upcomingMatches = allMatches.Where(c => c.MatchDate > DateTime.UtcNow).ToList();

            Matches.TodaysBestMatch = upcomingMatches?
             .Where(c => c.MatchDate <= DateTime.UtcNow.Date.AddHours(4))?
             .OrderByDescending(c => c.ExcitementScore)?
             .FirstOrDefault();

            if (Matches.TodaysBestMatch == null)
            {
                Matches.TodaysBestMatch = upcomingMatches?
               .Where(c => c.MatchDate.Date == DateTime.UtcNow.Date)?
               .OrderByDescending(c => c.ExcitementScore)?
               .FirstOrDefault();
            }

            if (Matches.TodaysBestMatch == null)
            {
                Matches.TodaysBestMatch = upcomingMatches?
                .Where(c => c.MatchDate.Date <= DateTime.UtcNow.AddHours(32))?
                .OrderByDescending(c => c.ExcitementScore)?
                .FirstOrDefault();
            }

            if (Matches.TodaysBestMatch != null)
                allMatches.Remove(Matches.TodaysBestMatch);
        }

        private Dictionary<int, League> PrepareLeagues(List<ExcitementMatch> allMatches)
        {
            return allMatches
                .GroupBy(c => c.League.Id)
                .Select(c => new KeyValuePair<int, League>(c.Key, c.First().League))
                .OrderBy(c => c.Key)
                .ToDictionary();
        }
    }
}
