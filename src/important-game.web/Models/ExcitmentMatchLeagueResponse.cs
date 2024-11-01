using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.web.Models
{
    public class ExcitmentMatchLeagueResponse
    {
        public Dictionary<LeagueDto, List<ExcitementMatchDto>> Leagues { get; set; } = new();
    }
}
