using important_game.infrastructure.ImportantMatch.Data.Entities;

namespace important_game.infrastructure.ImportantMatch.Live
{
    public interface IExcitmentMatchLiveProcessor
    {
        Task<LiveMatch?> ProcessLiveMatchData(Match match);
    }
}
