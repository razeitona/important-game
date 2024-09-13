using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.infrastructure.ImportantMatch
{
    public interface IExctimentMatchRepository
    {
        Task<List<ExcitementMatch>> GetAllMatches();
    }
}
