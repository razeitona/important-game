using important_game.ui.Core.Models;
using important_game.ui.Infrastructure.ImportantMatch;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IExcitmentMatchProcessor _excitmentMatchProcessor;
        public List<ExcitementMatch> Excitements { get; private set; } = new List<ExcitementMatch>();

        public IndexModel(ILogger<IndexModel> logger, IExcitmentMatchProcessor excitmentMatchProcessor)
        {
            _logger = logger;
            _excitmentMatchProcessor = excitmentMatchProcessor;
        }


        public async Task OnGet()
        {
            var excitementMatches = await _excitmentMatchProcessor.GetUpcomingExcitementMatchesAsync(new MatchImportanceOptions());

            Excitements = excitementMatches.OrderByDescending(c => c.ExcitementScore).ThenBy(c => c.MatchDate).ToList();
        }
    }
}
