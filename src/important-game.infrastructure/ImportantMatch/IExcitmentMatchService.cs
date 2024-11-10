using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.infrastructure.ImportantMatch
{
    public interface IExcitmentMatchService
    {
        Task CalculateUpcomingMatchsExcitment();
        Task CalculateUnfinishedMatchExcitment();
        Task<List<ExcitementMatchDto>> GetAllMatchesAsync();
        Task<List<ExcitementMatchLiveDto>> GetLiveMatchesAsync();
        Task<ExcitementMatchDetailDto> GetMatchByIdAsync(int id);
    }
}
