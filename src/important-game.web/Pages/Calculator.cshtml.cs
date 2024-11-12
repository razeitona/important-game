using important_game.infrastructure.ImportantMatch;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class CalculatorModel : PageModel
    {
        private readonly ILogger<CalculatorModel> _logger;
        private readonly IExcitmentMatchProcessor _matchProcessor;
        public CalculatorModel(ILogger<CalculatorModel> logger, IExcitmentMatchProcessor matchProcessor)
        {
            _logger = logger;
            _matchProcessor = matchProcessor;
        }

        public async Task OnGet()
        {
            await _matchProcessor.CalculateUpcomingMatchsExcitment();
        }

    }
}
