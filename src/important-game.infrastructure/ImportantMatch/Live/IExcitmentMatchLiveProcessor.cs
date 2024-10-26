namespace important_game.infrastructure.ImportantMatch.Live
{
    public interface IExcitmentMatchLiveProcessor
    {
        Task<double> ProcessLiveMatchData(long eventId);
    }
}
