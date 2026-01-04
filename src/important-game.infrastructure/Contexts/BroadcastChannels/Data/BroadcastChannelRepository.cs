using Dapper;
using important_game.infrastructure.Contexts.BroadcastChannels.Data.Entities;
using important_game.infrastructure.Contexts.BroadcastChannels.Data.Queries;
using important_game.infrastructure.Data.Connections;
using System.Diagnostics.CodeAnalysis;
using static Dapper.SqlMapper;

namespace important_game.infrastructure.Contexts.BroadcastChannels.Data;

[ExcludeFromCodeCoverage]
public class BroadcastChannelRepository(IDbConnectionFactory connectionFactory) : IBroadcastChannelRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    // ==================== Countries ====================

    public async Task<List<CountryEntity>> GetAllCountriesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<CountryEntity>(CountriesQueries.SelectAllCountries);
        return result.ToList();
    }

    public async Task<CountryEntity?> GetCountryByCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<CountryEntity>(
            CountriesQueries.SelectCountryByCode,
            new { CountryCode = countryCode });
        return result;
    }

    public async Task<List<CountryEntity>> GetBroadcastCountriesForMatchAsync(int matchId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<CountryEntity>(
            BroadcastMatchQueries.SelectBroadcastCountriesForMatch,
            new { MatchId = matchId });
        return result.ToList();
    }

    // ==================== Broadcast Channels ====================

    public async Task<BroadcastChannelEntity> UpsertBroadcastChannelAsync(BroadcastChannelEntity broadcastEntity)
    {
        ArgumentNullException.ThrowIfNull(broadcastEntity);

        using var connection = _connectionFactory.CreateConnection();
        var broadcastId = await connection.ExecuteScalarAsync<int>(BroadcastChannelQueries.CheckIfBroadcastExists,
            new { broadcastEntity.Code, broadcastEntity.CountryCode });

        if (broadcastId > 0)
        {
            await connection.ExecuteAsync(BroadcastChannelQueries.UpdateBroadcastChannel, new
            {
                ChannelId = broadcastId,
                broadcastEntity.Name,
                broadcastEntity.Code,
                broadcastEntity.CountryCode
            });
            broadcastEntity.ChannelId = broadcastId;
            return broadcastEntity;
        }
        else
        {
            var insertedId = await connection.ExecuteScalarAsync<int>(BroadcastChannelQueries.InsertBroadcastChannel, new
            {
                broadcastEntity.Name,
                broadcastEntity.Code,
                broadcastEntity.CountryCode
            });
            broadcastEntity.ChannelId = insertedId;
            return broadcastEntity;
        }
    }

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
            BroadcastMatchQueries.SelectBroadcastsByMatchId,
            new { MatchId = matchId });
        return result.ToList();
    }

    public async Task<Dictionary<int, List<MatchBroadcastDto>>> GetBroadcastsByMatchIdsAsync(List<int> matchIds, CancellationToken cancellationToken = default)
    {
        if (matchIds == null || matchIds.Count == 0)
            return new Dictionary<int, List<MatchBroadcastDto>>();

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<MatchBroadcastDto>(
            BroadcastMatchQueries.SelectBroadcastsByMatchIds,
            new { MatchIds = matchIds });

        return result
            .GroupBy(b => b.MatchId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<List<MatchBroadcastDto>> GetBroadcastsByChannelIdAsync(int channelId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<MatchBroadcastDto>(
            BroadcastMatchQueries.SelectBroadcastsByChannelId,
            new { ChannelId = channelId });
        return result.ToList();
    }

    public async Task UpsertBroadcastMatchAsync(List<MatchBroadcastEntity> matchBroadCasts)
    {
        ArgumentNullException.ThrowIfNull(matchBroadCasts);

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(BroadcastMatchQueries.DeletePasthMatchBroadcasts, new { CurrentUtcDate = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss") });
        await connection.ExecuteAsync(BroadcastMatchQueries.UpsertBroadcastMatches, matchBroadCasts);

    }

    // ==================== User Favorite Channels ====================

    public async Task<List<int>> GetUserFavoriteChannelIdsAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<int>(
            UserFavoriteChannelsQueries.SelectUserFavoriteChannelIds,
            new { UserId = userId });
        return result.ToList();
    }

    public async Task<List<BroadcastChannelEntity>> GetUserFavoriteChannelsAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<BroadcastChannelEntity>(
            UserFavoriteChannelsQueries.SelectUserFavoriteChannels,
            new { UserId = userId });
        return result.ToList();
    }

    public async Task<bool> IsFavoriteChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            UserFavoriteChannelsQueries.CheckIsFavoriteChannel,
            new { UserId = userId, ChannelId = channelId });
        return count > 0;
    }

    public async Task AddUserFavoriteChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            UserFavoriteChannelsQueries.InsertUserFavoriteChannel,
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
            UserFavoriteChannelsQueries.DeleteUserFavoriteChannel,
            new { UserId = userId, ChannelId = channelId });
    }
}
