using important_game.infrastructure.Contexts.Matches.Data;
using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Contexts.Matches;
internal class MatchesService(IMatchesRepository matchesRepository) : IMatchesService
{

    public async Task<List<MatchDto>> GetAllUpcomingMatchesAsync(CancellationToken cancellationToken = default)
    {
        var upcomingMatches = await matchesRepository.GetAllUpcomingMatchesAsync();
        return upcomingMatches;
    }
}
