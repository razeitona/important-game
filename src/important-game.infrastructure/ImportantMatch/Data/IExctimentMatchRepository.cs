using important_game.infrastructure.ImportantMatch.Data.Entities;

namespace important_game.infrastructure.ImportantMatch.Data
{
    public interface IExctimentMatchRepository
    {
        Task<List<ExcitementMatch>> GetAllMatchesAsync();
        Task<ExcitementMatch> GetMatchByIdAsync(int id);
        Task SaveMatchesAsync(List<ExcitementMatch> excitementMatches);
    }
}
