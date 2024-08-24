using important_game.ui.Core.Models;

namespace important_game.ui.Infrastructure.ImportantMatch
{
    public interface IExcitmentMatchProcessor
    {
        Task<List<ExcitementMatch>> GetUpcomingExcitementMatchesAsync(MatchImportanceOptions options);
    }
}
