using important_game.infrastructure.Contexts.BroadcastChannels.Data.Entities;

namespace important_game.infrastructure.Contexts.BroadcastChannels.Data;

public interface IBroadcastChannelRepository
{
    // Countries
    Task<List<CountryEntity>> GetAllCountriesAsync(CancellationToken cancellationToken = default);
    Task<CountryEntity?> GetCountryByCodeAsync(string countryCode, CancellationToken cancellationToken = default);
    Task<List<CountryEntity>> GetBroadcastCountriesForMatchAsync(int matchId, CancellationToken cancellationToken = default);

    // Broadcast Channels
    Task<BroadcastChannelEntity> UpsertBroadcastChannelAsync(BroadcastChannelEntity broadcastEntity);
    Task<List<BroadcastChannelEntity>> GetAllActiveChannelsAsync(CancellationToken cancellationToken = default);
    Task<BroadcastChannelEntity?> GetChannelByIdAsync(int channelId, CancellationToken cancellationToken = default);
    Task<List<BroadcastChannelEntity>> GetChannelsByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default);
    Task<List<BroadcastChannelEntity>> GetChannelsByIdsAsync(List<int> channelIds, CancellationToken cancellationToken = default);

    // Match Broadcasts
    Task<List<MatchBroadcastDto>> GetBroadcastsByMatchIdAsync(int matchId, CancellationToken cancellationToken = default);
    Task<Dictionary<int, List<MatchBroadcastDto>>> GetBroadcastsByMatchIdsAsync(List<int> matchIds, CancellationToken cancellationToken = default);
    Task<List<MatchBroadcastDto>> GetBroadcastsByChannelIdAsync(int channelId, CancellationToken cancellationToken = default);
    Task UpsertBroadcastMatchAsync(List<MatchBroadcastEntity> matchBroadCasts);

    // User Favorite Channels
    Task<List<int>> GetUserFavoriteChannelIdsAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<BroadcastChannelEntity>> GetUserFavoriteChannelsAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> IsFavoriteChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default);
    Task AddUserFavoriteChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default);
    Task RemoveUserFavoriteChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default);
}
