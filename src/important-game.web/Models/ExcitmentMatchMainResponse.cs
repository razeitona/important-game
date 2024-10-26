using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.web.Models
{
    public class ExcitmentMatchMainResponse
    {
        public List<ExcitementMatch> WeekMatches { get; set; }
        public List<LiveExcitementMatch> LiveGames { get; set; }
        public List<ExcitementMatch> UpcomingMatch { get; set; }
    }
}
