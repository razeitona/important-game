using important_game.infrastructure.ImportantMatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class LiveMatchCalculatorModel(IExcitmentMatchService matchService) : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            await matchService.CalculateUnfinishedMatchExcitment();
            return new JsonResult("");
        }
    }
}
