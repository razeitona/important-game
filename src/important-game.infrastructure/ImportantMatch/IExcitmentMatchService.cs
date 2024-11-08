using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.infrastructure.ImportantMatch
{
    public interface IExcitmentMatchService
    {
        Task CalculateUpcomingMatchsExcitment();
        Task CalculateLiveMatchExcitment();
        Task<List<ExcitementMatchDto>> GetAllMatchesAsync();
        List<ExcitementMatchLiveDto> GetLiveMatchesAsync();
        ExcitementMatchDetailDto GetMatchByIdAsync(int id);
    }
}
