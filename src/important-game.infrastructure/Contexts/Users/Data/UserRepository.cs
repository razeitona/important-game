using Dapper;
using important_game.infrastructure.Contexts.Users.Data.Entities;
using important_game.infrastructure.Contexts.Users.Data.Queries;
using important_game.infrastructure.Data.Connections;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Users.Data;

[ExcludeFromCodeCoverage]
public class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    public async Task<UserEntity?> GetUserByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<UserEntity>(
            UserQueries.GetUserByGoogleId,
            new { GoogleId = googleId });
        return result;
    }

    public async Task<UserEntity?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<UserEntity>(
            UserQueries.GetUserById,
            new { UserId = userId });
        return result;
    }

    public async Task<int> CreateUserAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        using var connection = _connectionFactory.CreateConnection();
        var userId = await connection.ExecuteScalarAsync<int>(
            UserQueries.CreateUser,
            new
            {
                user.GoogleId,
                user.Email,
                user.Name,
                user.ProfilePictureUrl,
                user.PreferredTimezone,
                CreatedAt = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                LastLoginAt = user.LastLoginAt?.ToString("yyyy-MM-dd HH:mm:ss")
            });
        return userId;
    }

    public async Task UpdateLastLoginAsync(int userId, DateTime lastLoginAt, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            UserQueries.UpdateLastLogin,
            new
            {
                UserId = userId,
                LastLoginAt = lastLoginAt.ToString("yyyy-MM-dd HH:mm:ss")
            });
    }

    public async Task UpdateUserPreferencesAsync(int userId, string timezone, string? name, string? profilePictureUrl, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            UserQueries.UpdateUserPreferences,
            new
            {
                UserId = userId,
                PreferredTimezone = timezone,
                Name = name,
                ProfilePictureUrl = profilePictureUrl
            });
    }

    // Favorite Matches
    public async Task<List<UserFavoriteMatchEntity>> GetUserFavoriteMatchesAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<UserFavoriteMatchEntity>(
            UserQueries.GetUserFavoriteMatches,
            new { UserId = userId });
        return result.ToList();
    }

    public async Task AddFavoriteMatchAsync(int userId, int matchId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            UserQueries.AddFavoriteMatch,
            new
            {
                UserId = userId,
                MatchId = matchId,
                AddedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            });
    }

    public async Task RemoveFavoriteMatchAsync(int userId, int matchId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            UserQueries.RemoveFavoriteMatch,
            new { UserId = userId, MatchId = matchId });
    }

    public async Task<bool> IsFavoriteMatchAsync(int userId, int matchId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            UserQueries.IsFavoriteMatch,
            new { UserId = userId, MatchId = matchId });
        return count > 0;
    }

    // Match Votes
    public async Task<MatchVoteEntity?> GetUserVoteAsync(int userId, int matchId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<MatchVoteEntity>(
            UserQueries.GetUserVote,
            new { UserId = userId, MatchId = matchId });
        return result;
    }

    public async Task AddOrUpdateVoteAsync(int userId, int matchId, int voteType, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            UserQueries.AddOrUpdateVote,
            new
            {
                UserId = userId,
                MatchId = matchId,
                VoteType = voteType,
                VotedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            });
    }

    public async Task RemoveVoteAsync(int userId, int matchId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            UserQueries.RemoveVote,
            new { UserId = userId, MatchId = matchId });
    }

    public async Task<int> GetMatchVoteCountAsync(int matchId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<int>(
            UserQueries.GetMatchVoteCount,
            new { MatchId = matchId });

        return result;
    }

    public async Task<Dictionary<int, MatchVoteEntity>> GetUserVotesForMatchesAsync(int userId, List<int> matchIds, CancellationToken cancellationToken = default)
    {
        if (matchIds == null || matchIds.Count == 0)
            return new Dictionary<int, MatchVoteEntity>();

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<MatchVoteEntity>(
            UserQueries.GetUserVotesForMatches,
            new { UserId = userId, MatchIds = matchIds });

        return result.ToDictionary(v => v.MatchId);
    }
}
