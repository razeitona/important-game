using important_game.ui.Infrastructure.ImportantMatch;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace important_game.web.Pages
{
    public class CalculatorModel : PageModel
    {
        private readonly ILogger<CalculatorModel> _logger;
        private readonly IExcitmentMatchProcessor _excitmentMatchProcessor;
        public CalculatorModel(ILogger<CalculatorModel> logger, IExcitmentMatchProcessor excitmentMatchProcessor)
        {
            _logger = logger;
            _excitmentMatchProcessor = excitmentMatchProcessor;
        }


        public async Task OnGet()
        {

            var excitementMatches = await _excitmentMatchProcessor.GetUpcomingExcitementMatchesAsync(new MatchImportanceOptions());

            if (excitementMatches == null)
                return;

            await System.IO.File.WriteAllTextAsync("data.json", JsonSerializer.Serialize(excitementMatches));

        }
    }
}
