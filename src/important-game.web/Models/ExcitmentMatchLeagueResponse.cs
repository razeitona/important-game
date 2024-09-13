using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.web.Models
{
    public class ExcitmentMatchLeagueResponse
    {
        public Dictionary<League, List<ExcitementMatch>> Leagues { get; set; } = new();
    }
}
