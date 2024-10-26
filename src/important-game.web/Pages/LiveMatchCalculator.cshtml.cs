using important_game.infrastructure.ImportantMatch.Live;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class LiveMatchCalculatorModel : PageModel
    {
        private readonly ILogger<CalculatorModel> _logger;
        private readonly IExcitmentMatchLiveProcessor _liveProcessor;
        public LiveMatchCalculatorModel(ILogger<CalculatorModel> logger, IExcitmentMatchLiveProcessor liveProcessor)
        {
            _logger = logger;
            _liveProcessor = liveProcessor;
        }

        public async Task OnGet()
        {
            //var stats = await _liveProcessor.ProcessLiveMatchData("12764383");
        }
    }
}
