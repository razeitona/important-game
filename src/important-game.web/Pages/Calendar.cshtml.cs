using important_game.infrastructure.ImportantMatch;
using important_game.web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class CalendarModel(ILogger<CalendarModel> _logger, IExcitmentMatchService _matchService) : PageModel
    {
        public ExcitmentMatchCalendarResponse Matches { get; private set; } = new ExcitmentMatchCalendarResponse();


        public async Task OnGet()
        {
            var allMatches = await _matchService.GetAllMatches();

            var matchesGroupedMonth = allMatches.GroupBy(c => c.MatchDate.Date.Month).OrderBy(c => c.Key);

            foreach (var month in matchesGroupedMonth)
            {
                Matches.Dates.Add(new ExcitmentMatchCalendarItem
                {
                    Month = new DateTime(DateTime.Today.Year, month.Key, 1),
                    Days = month.GroupBy(c => c.MatchDate.Date).OrderBy(c => c.Key).ToDictionary(c => c.Key, v => v.OrderBy(c => c.MatchDate).ToList())
                });
            }

        }
    }
}
