using Dapper;
using important_game.infrastructure.Contexts.BroadcastChannels.Data.Entities;
using important_game.infrastructure.Contexts.BroadcastChannels.Data.Queries;
using important_game.infrastructure.Data.Connections;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.BroadcastChannels.Data;

[ExcludeFromCodeCoverage]
public class BroadcastChannelRepository(IDbConnectionFactory connectionFactory) : IBroadcastChannelRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    // ==================== Countries ====================

    public async Task<List<CountryEntity>> GetAllCountriesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<CountryEntity>(BroadcastChannelQueries.SelectAllCountries);
        return result.ToList();
    }

    public async Task<CountryEntity?> GetCountryByCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<CountryEntity>(
            BroadcastChannelQueries.SelectCountryByCode,
            new { CountryCode = countryCode });
        return result;
    }

    public async Task<List<CountryEntity>> GetBroadcastCountriesForMatchAsync(int matchId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<CountryEntity>(
            BroadcastChannelQueries.SelectBroadcastCountriesForMatch,
            new { MatchId = matchId });
        return result.ToList();
    }

    // ==================== Broadcast Channels ====================

    public async Task<List<BroadcastChannelEntity>> GetAllActiveChannelsAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<BroadcastChannelEntity>(
            BroadcastChannelQueries.SelectAllActiveChannels);
        return result.ToList();
    }

    public async Task<BroadcastChannelEntity?> GetChannelByIdAsync(int channelId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<BroadcastChannelEntity>(
            BroadcastChannelQueries.SelectChannelById,
            new { ChannelId = channelId });
        return result;
    }

    public async Task<List<BroadcastChannelEntity>> GetChannelsByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<BroadcastChannelEntity>(
            BroadcastChannelQueries.SelectChannelsByCountryCode,
            new { CountryCode = countryCode });
        return result.ToList();
    }

    public async Task<List<BroadcastChannelEntity>> GetChannelsByIdsAsync(List<int> channelIds, CancellationToken cancellationToken = default)
    {
        if (channelIds == null || channelIds.Count == 0)
            return new List<BroadcastChannelEntity>();

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<BroadcastChannelEntity>(
            BroadcastChannelQueries.SelectChannelsByIds,
            new { ChannelIds = channelIds });
        return result.ToList();
    }

    // ==================== Match Broadcasts ====================

    public async Task<List<MatchBroadcastDto>> GetBroadcastsByMatchIdAsync(int matchId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<MatchBroadcastDto>(
            BroadcastChannelQueries.SelectBroadcastsByMatchId,
            new { MatchId = matchId });
        return result.ToList();
    }

    public async Task<Dictionary<int, List<MatchBroadcastDto>>> GetBroadcastsByMatchIdsAsync(List<int> matchIds, CancellationToken cancellationToken = default)
    {
        if (matchIds == null || matchIds.Count == 0)
            return new Dictionary<int, List<MatchBroadcastDto>>();

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<MatchBroadcastDto>(
            BroadcastChannelQueries.SelectBroadcastsByMatchIds,
            new { MatchIds = matchIds });

        return result
            .GroupBy(b => b.MatchId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<List<MatchBroadcastDto>> GetBroadcastsByChannelIdAsync(int channelId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<MatchBroadcastDto>(
            BroadcastChannelQueries.SelectBroadcastsByChannelId,
            new { ChannelId = channelId });
        return result.ToList();
    }

    public async Task AddMatchBroadcastAsync(MatchBroadcastEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            BroadcastChannelQueries.InsertMatchBroadcast,
            new
            {
                entity.MatchId,
                entity.ChannelId,
                CreatedAt = entity.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });
    }

    public async Task RemoveMatchBroadcastAsync(int matchId, int channelId, string countryCode, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            BroadcastChannelQueries.DeleteMatchBroadcast,
            new { MatchId = matchId, ChannelId = channelId, CountryCode = countryCode });
    }

    public async Task RemoveAllMatchBroadcastsAsync(int matchId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            BroadcastChannelQueries.DeleteMatchBroadcastsByMatchId,
            new { MatchId = matchId });
    }

    // ==================== User Favorite Channels ====================

    public async Task<List<int>> GetUserFavoriteChannelIdsAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<int>(
            BroadcastChannelQueries.SelectUserFavoriteChannelIds,
            new { UserId = userId });
        return result.ToList();
    }

    public async Task<List<BroadcastChannelEntity>> GetUserFavoriteChannelsAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<BroadcastChannelEntity>(
            BroadcastChannelQueries.SelectUserFavoriteChannels,
            new { UserId = userId });
        return result.ToList();
    }

    public async Task<bool> IsFavoriteChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            BroadcastChannelQueries.CheckIsFavoriteChannel,
            new { UserId = userId, ChannelId = channelId });
        return count > 0;
    }

    public async Task AddUserFavoriteChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            BroadcastChannelQueries.InsertUserFavoriteChannel,
            new
            {
                UserId = userId,
                ChannelId = channelId,
                AddedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            });
    }

    public async Task RemoveUserFavoriteChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            BroadcastChannelQueries.DeleteUserFavoriteChannel,
            new { UserId = userId, ChannelId = channelId });
    }
}
