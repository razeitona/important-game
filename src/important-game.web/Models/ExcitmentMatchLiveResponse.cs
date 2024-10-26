using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.web.Models
{
    public class ExcitmentMatchLiveResponse
    {
        public List<LiveExcitementMatch> Matches { get; set; } = new();
    }

}
