using important_game.ui.Core.Models;
using important_game.ui.Infrastructure.ImportantMatch;
using important_game.web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace important_game.web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IExcitmentMatchProcessor _excitmentMatchProcessor;
        public ExcitmentMatchResponse Matches { get; private set; } = new ExcitmentMatchResponse();

        public IndexModel(ILogger<IndexModel> logger, IExcitmentMatchProcessor excitmentMatchProcessor)
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
                excitementMatches = await _excitmentMatchProcessor.GetUpcomingExcitementMatchesAsync(new MatchImportanceOptions());

            if (excitementMatches == null)
                return;

            var allMatches = excitementMatches!.Where(c => c.MatchDate > DateTime.UtcNow).OrderBy(c => c.MatchDate).ToList();

            Matches.Leagues = PrepareLeagues(allMatches);

            Matches.TodaysMatch = allMatches?
                .Where(c => c.MatchDate <= DateTime.UtcNow.Date.AddHours(32))?
                .OrderByDescending(c => c.ExcitementScore)?
                .FirstOrDefault();

            allMatches.Remove(Matches.TodaysMatch);

            Matches.UpcomingMatch = allMatches.OrderByDescending(c => c.ExcitementScore).ToList();

            await System.IO.File.WriteAllTextAsync("data.json", JsonSerializer.Serialize(excitementMatches));

        }

        private Dictionary<int, League> PrepareLeagues(List<ExcitementMatch> allMatches)
        {
            return allMatches.GroupBy(c => c.League.Id).Select(c => new KeyValuePair<int, League>(c.Key, c.First().League)).ToDictionary();
        }
    }
}
