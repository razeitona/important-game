using important_game.infrastructure.Contexts.BroadcastChannels.Models;

namespace important_game.infrastructure.Contexts.BroadcastChannels;

public interface IBroadcastChannelService
{
    // Countries
    Task<List<CountryViewModel>> GetAllCountriesAsync(CancellationToken cancellationToken = default);
    Task<List<CountryViewModel>> GetBroadcastCountriesForMatchAsync(int matchId, CancellationToken cancellationToken = default);

    // Broadcast Channels
    Task<List<BroadcastChannelViewModel>> GetAllActiveChannelsAsync(CancellationToken cancellationToken = default);
    Task<List<BroadcastChannelViewModel>> GetChannelsByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default);
    Task<Dictionary<string, List<BroadcastChannelViewModel>>> GetChannelsGroupedByCountryAsync(CancellationToken cancellationToken = default);

    // Match Broadcasts
    Task<List<MatchBroadcastViewModel>> GetBroadcastsByMatchIdAsync(int matchId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, List<MatchBroadcastViewModel>>> GetBroadcastsByMatchIdGroupedByCountryAsync(int matchId, CancellationToken cancellationToken = default);
    Task<Dictionary<int, List<MatchBroadcastViewModel>>> GetBroadcastsByMatchIdsAsync(List<int> matchIds, CancellationToken cancellationToken = default);

    // User Favorite Channels
    Task<List<int>> GetUserFavoriteChannelIdsAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<BroadcastChannelViewModel>> GetUserFavoriteChannelsAsync(int userId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, List<BroadcastChannelViewModel>>> GetUserFavoriteChannelsGroupedByCountryAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> IsFavoriteChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default);
    Task AddUserFavoriteChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default);
    Task RemoveUserFavoriteChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default);

    // TV Listings
    Task<TvListingsViewModel> GetTvListingsAsync(DateTime startDate, DateTime endDate, int? userId = null, CancellationToken cancellationToken = default);
}
