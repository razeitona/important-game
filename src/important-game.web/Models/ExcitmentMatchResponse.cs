using important_game.ui.Core.Models;

namespace important_game.web.Models
{
    public class ExcitmentMatchResponse
    {
        public ExcitementMatch TodaysBestMatch { get; set; }
        public List<ExcitementMatch> UpcomingMatch { get; set; }
        public List<ExcitementMatch> LiveGames { get; set; }

        public Dictionary<int, League> Leagues { get; set; }
    }
}
