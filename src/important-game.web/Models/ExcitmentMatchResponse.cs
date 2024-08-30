using important_game.ui.Core.Models;

namespace important_game.web.Models
{
    public class ExcitmentMatchResponse
    {
        public ExcitementMatch TodaysMatch { get; set; }
        public List<ExcitementMatch> UpcomingMatch { get; set; }

        public Dictionary<int, League> Leagues { get; set; }
    }
}
