using important_game.infrastructure.Contexts.Users.Data.Entities;

namespace important_game.infrastructure.Contexts.Users.Data;

public interface IUserRepository
{
    Task<UserEntity?> GetUserByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> CreateUserAsync(UserEntity user, CancellationToken cancellationToken = default);
    Task UpdateLastLoginAsync(int userId, DateTime lastLoginAt, CancellationToken cancellationToken = default);
    Task UpdateUserPreferencesAsync(int userId, string timezone, string? name, string? profilePictureUrl, CancellationToken cancellationToken = default);

    // Favorite Matches
    Task<List<UserFavoriteMatchEntity>> GetUserFavoriteMatchesAsync(int userId, CancellationToken cancellationToken = default);
    Task AddFavoriteMatchAsync(int userId, int matchId, CancellationToken cancellationToken = default);
    Task RemoveFavoriteMatchAsync(int userId, int matchId, CancellationToken cancellationToken = default);
    Task<bool> IsFavoriteMatchAsync(int userId, int matchId, CancellationToken cancellationToken = default);

    // Match Votes
    Task<MatchVoteEntity?> GetUserVoteAsync(int userId, int matchId, CancellationToken cancellationToken = default);
    Task AddOrUpdateVoteAsync(int userId, int matchId, int voteType, CancellationToken cancellationToken = default);
    Task RemoveVoteAsync(int userId, int matchId, CancellationToken cancellationToken = default);
    Task<int> GetMatchVoteCountAsync(int matchId, CancellationToken cancellationToken = default);
    Task<Dictionary<int, MatchVoteEntity>> GetUserVotesForMatchesAsync(int userId, List<int> matchIds, CancellationToken cancellationToken = default);
}
