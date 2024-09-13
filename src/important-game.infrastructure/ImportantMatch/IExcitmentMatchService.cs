using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.infrastructure.ImportantMatch
{
    public interface IExcitmentMatchService
    {
        Task<List<ExcitementMatch>> GetAllMatches();
    }
}
