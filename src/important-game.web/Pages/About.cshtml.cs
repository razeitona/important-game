using important_game.infrastructure.ImportantMatch;
using important_game.infrastructure.ImportantMatch.Live;
using important_game.infrastructure.ImportantMatch.Models;
using important_game.web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace important_game.web.Pages
{
    public class AboutModel(ILogger<AboutModel> _logger) : PageModel
    {
        public async Task OnGet()
        {
        }

    }
}
