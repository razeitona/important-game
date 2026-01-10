using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Models;

namespace important_game.infrastructure.Contexts.Matches;
public interface IMatchService
{
    Task<List<MatchDto>> GetAllUnfinishedMatchesAsync(CancellationToken cancellationToken = default);
    Task<List<MatchDto>> GetUserFavoriteUpcomingMatchesAsync(int userId, CancellationToken cancellationToken = default);
    Task<MatchesViewModel> GetAllMatchesAsync(CancellationToken cancellationToken = default);
    Task<MatchDetailViewModel?> GetMatchByIdAsync(int matchId, CancellationToken cancellationToken = default);
    Task<MatchDetailViewModel?> GetMatchByTeamSlugsAsync(string homeSlug, string awaySlug, CancellationToken cancellationToken = default);
    Task<MatchDetailViewModel?> GetMatchOfTheDayAsync(CancellationToken cancellationToken = default);
}
