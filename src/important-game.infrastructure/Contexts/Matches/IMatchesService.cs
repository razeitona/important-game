using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Contexts.Matches;
public interface IMatchesService
{
    Task<List<MatchDto>> GetAllUpcomingMatchesAsync(CancellationToken cancellationToken = default);
    Task<MatchDetailDto> GetMatchByIdAsync(int id);
}
