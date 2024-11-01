using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.web.Models
{
    public class ExcitmentMatchesResponse
    {
        public List<LeagueDto> Leagues { get; set; } = new();
        public List<ExcitementMatchDto> Matches { get; set; } = new();
    }
}
