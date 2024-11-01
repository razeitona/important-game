using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.web.Models
{
    public class ExcitmentMatchLiveResponse
    {
        public List<ExcitementMatchDto> Matches { get; set; } = new();
    }

}
