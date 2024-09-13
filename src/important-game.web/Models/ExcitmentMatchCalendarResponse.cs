using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.web.Models
{
    public class ExcitmentMatchCalendarResponse
    {
        public List<ExcitmentMatchCalendarItem> Dates { get; set; } = new();
    }

    public class ExcitmentMatchCalendarItem
    {
        public DateTime Month { get; set; }
        public Dictionary<DateTime, List<ExcitementMatch>> Days { get; set; }
    }
}
