using important_game.infrastructure.Contexts.Users.Data.Entities;
using important_game.infrastructure.Contexts.Users.Models;

namespace important_game.infrastructure.Contexts.Users;

public interface IUserService
{
    Task<UserEntity?> GetOrCreateUserFromGoogleAsync(string googleId, string email, string? name, string? pictureUrl, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task UpdateLastLoginAsync(int userId, CancellationToken cancellationToken = default);
    Task UpdateUserPreferencesAsync(int userId, UserPreferencesDto preferences, CancellationToken cancellationToken = default);

    // Favorite Matches
    Task<List<int>> GetUserFavoriteMatchIdsAsync(int userId, CancellationToken cancellationToken = default);
    Task AddFavoriteMatchAsync(int userId, int matchId, CancellationToken cancellationToken = default);
    Task RemoveFavoriteMatchAsync(int userId, int matchId, CancellationToken cancellationToken = default);
    Task<bool> IsFavoriteMatchAsync(int userId, int matchId, CancellationToken cancellationToken = default);

    // Match Votes
    Task<MatchVoteDto?> GetUserVoteAsync(int userId, int matchId, CancellationToken cancellationToken = default);
    Task ToggleVoteAsync(int userId, int matchId, CancellationToken cancellationToken = default);
    Task<MatchVoteStatsDto> GetMatchVoteStatsAsync(int matchId, CancellationToken cancellationToken = default);
    Task<Dictionary<int, bool>> GetUserVotesForMatchesAsync(int userId, List<int> matchIds, CancellationToken cancellationToken = default);
    Task AddOrUpdateVoteAsync(int userId, int matchId, int voteType, CancellationToken cancellationToken = default);
    Task RemoveVoteAsync(int userId, int matchId, CancellationToken cancellationToken = default);
    Task<int> GetMatchVoteCountAsync(int matchId, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetUserByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);

    // Favorite Teams
    Task<List<int>> GetUserFavoriteTeamIdsAsync(int userId, CancellationToken cancellationToken = default);
    Task AddFavoriteTeamAsync(int userId, int teamId, CancellationToken cancellationToken = default);
    Task RemoveFavoriteTeamAsync(int userId, int teamId, CancellationToken cancellationToken = default);
    Task<bool> IsFavoriteTeamAsync(int userId, int teamId, CancellationToken cancellationToken = default);
}
