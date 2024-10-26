using important_game.infrastructure.ImportantMatch;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class CalculatorModel : PageModel
    {
        private readonly ILogger<CalculatorModel> _logger;
        private readonly IExcitmentMatchService _excitmentMatchService;
        public CalculatorModel(ILogger<CalculatorModel> logger, IExcitmentMatchService excitmentMatchService)
        {
            _logger = logger;
            _excitmentMatchService = excitmentMatchService;
        }

        public async Task OnGet()
        {
            await _excitmentMatchService.CalculateUpcomingMatchsExcitment();
        }
    }
}
