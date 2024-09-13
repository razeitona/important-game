using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.infrastructure.ImportantMatch
{
    public interface IExcitmentMatchProcessor
    {
        Task<List<ExcitementMatch>> GetUpcomingExcitementMatchesAsync(ExctimentMatchOptions options);
    }
}
