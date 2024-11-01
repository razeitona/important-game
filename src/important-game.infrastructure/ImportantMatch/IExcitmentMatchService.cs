using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.infrastructure.ImportantMatch
{
    public interface IExcitmentMatchService
    {
        Task CalculateUpcomingMatchsExcitment();
        Task<List<ExcitementMatchDto>> GetAllMatchesAsync();
        Task<ExcitementMatchDetailDto> GetMatchByIdAsync(int id);
    }
}
