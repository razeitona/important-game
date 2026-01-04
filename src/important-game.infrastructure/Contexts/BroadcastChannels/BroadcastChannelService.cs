using important_game.infrastructure.Contexts.BroadcastChannels.Data;
using important_game.infrastructure.Contexts.BroadcastChannels.Mappers;
using important_game.infrastructure.Contexts.BroadcastChannels.Models;
using important_game.infrastructure.Contexts.Matches.Data;
using Microsoft.Extensions.Caching.Memory;

namespace important_game.infrastructure.Contexts.BroadcastChannels;

internal class BroadcastChannelService(
    IBroadcastChannelRepository broadcastChannelRepository,
    IMatchesRepository matchesRepository,
    IMemoryCache memoryCache) : IBroadcastChannelService
{
    private readonly IBroadcastChannelRepository _broadcastChannelRepository = broadcastChannelRepository ?? throw new ArgumentNullException(nameof(broadcastChannelRepository));
    private readonly IMatchesRepository _matchesRepository = matchesRepository ?? throw new ArgumentNullException(nameof(matchesRepository));
    private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

    // ==================== Countries ====================

    public async Task<List<CountryViewModel>> GetAllCountriesAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "all_countries";
        var countries = _memoryCache.Get<List<CountryViewModel>>(cacheKey);

        if (countries == null || countries.Count == 0)
        {
            var entities = await _broadcastChannelRepository.GetAllCountriesAsync(cancellationToken);
            countries = entities.Select(BroadcastChannelMapper.MapToCountryViewModel).ToList();
            _memoryCache.Set(cacheKey, countries, TimeSpan.FromHours(24));
        }

        return countries;
    }

    public async Task<List<CountryViewModel>> GetBroadcastCountriesForMatchAsync(int matchId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"match_{matchId}_countries";
        var countries = _memoryCache.Get<List<CountryViewModel>>(cacheKey);

        if (countries == null || countries.Count == 0)
        {
            var entities = await _broadcastChannelRepository.GetBroadcastCountriesForMatchAsync(matchId, cancellationToken);
            countries = entities.Select(BroadcastChannelMapper.MapToCountryViewModel).ToList();
            _memoryCache.Set(cacheKey, countries, TimeSpan.FromMinutes(30));
        }

        return countries;
    }

    // ==================== Broadcast Channels ====================

    public async Task<List<BroadcastChannelViewModel>> GetAllActiveChannelsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "all_active_channels";
        var channels = _memoryCache.Get<List<BroadcastChannelViewModel>>(cacheKey);

        if (channels == null || channels.Count == 0)
        {
            var entities = await _broadcastChannelRepository.GetAllActiveChannelsAsync(cancellationToken);
            channels = BroadcastChannelMapper.MapToViewModels(entities);
            _memoryCache.Set(cacheKey, channels, TimeSpan.FromHours(1));
        }

        return channels;
    }

    public async Task<List<BroadcastChannelViewModel>> GetChannelsByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"channels_country_{countryCode}";
        var channels = _memoryCache.Get<List<BroadcastChannelViewModel>>(cacheKey);

        if (channels == null || channels.Count == 0)
        {
            var entities = await _broadcastChannelRepository.GetChannelsByCountryCodeAsync(countryCode, cancellationToken);
            channels = BroadcastChannelMapper.MapToViewModels(entities);
            _memoryCache.Set(cacheKey, channels, TimeSpan.FromHours(1));
        }

        return channels;
    }

    public async Task<Dictionary<string, List<BroadcastChannelViewModel>>> GetChannelsGroupedByCountryAsync(CancellationToken cancellationToken = default)
    {
        var groupedChannels = new Dictionary<string, List<BroadcastChannelViewModel>>();

        // Get all countries
        var countries = await _broadcastChannelRepository.GetAllCountriesAsync(cancellationToken);

        // For each country, get its channels
        foreach (var country in countries.OrderBy(c => c.CountryName))
        {
            var channels = await GetChannelsByCountryCodeAsync(country.CountryCode, cancellationToken);
            if (channels.Count > 0)
            {
                groupedChannels[country.CountryName] = channels;
            }
        }

        return groupedChannels;
    }

    // ==================== Match Broadcasts ====================

    public async Task<List<MatchBroadcastViewModel>> GetBroadcastsByMatchIdAsync(int matchId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"match_broadcasts_{matchId}";
        var broadcasts = _memoryCache.Get<List<MatchBroadcastViewModel>>(cacheKey);

        if (broadcasts == null || broadcasts.Count == 0)
        {
            var dtos = await _broadcastChannelRepository.GetBroadcastsByMatchIdAsync(matchId, cancellationToken);
            broadcasts = BroadcastChannelMapper.MapToMatchBroadcastViewModels(dtos);
            _memoryCache.Set(cacheKey, broadcasts, TimeSpan.FromMinutes(30));
        }

        return broadcasts;
    }

    public async Task<Dictionary<string, List<MatchBroadcastViewModel>>> GetBroadcastsByMatchIdGroupedByCountryAsync(int matchId, CancellationToken cancellationToken = default)
    {
        var broadcasts = await GetBroadcastsByMatchIdAsync(matchId, cancellationToken);
        return BroadcastChannelMapper.GroupBroadcastsByCountry(broadcasts);
    }

    public async Task<Dictionary<int, List<MatchBroadcastViewModel>>> GetBroadcastsByMatchIdsAsync(List<int> matchIds, CancellationToken cancellationToken = default)
    {
        if (matchIds == null || matchIds.Count == 0)
            return new Dictionary<int, List<MatchBroadcastViewModel>>();

        var result = await _broadcastChannelRepository.GetBroadcastsByMatchIdsAsync(matchIds, cancellationToken);

        return result.ToDictionary(
            kvp => kvp.Key,
            kvp => BroadcastChannelMapper.MapToMatchBroadcastViewModels(kvp.Value)
        );
    }

    // ==================== User Favorite Channels ====================

    public async Task<List<int>> GetUserFavoriteChannelIdsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"user_{userId}_favorite_channel_ids";
        var channelIds = _memoryCache.Get<List<int>>(cacheKey);

        if (channelIds == null)
        {
            channelIds = await _broadcastChannelRepository.GetUserFavoriteChannelIdsAsync(userId, cancellationToken);
            _memoryCache.Set(cacheKey, channelIds, TimeSpan.FromMinutes(15));
        }

        return channelIds;
    }

    public async Task<List<BroadcastChannelViewModel>> GetUserFavoriteChannelsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"user_{userId}_favorite_channels";
        var channels = _memoryCache.Get<List<BroadcastChannelViewModel>>(cacheKey);

        if (channels == null)
        {
            var entities = await _broadcastChannelRepository.GetUserFavoriteChannelsAsync(userId, cancellationToken);
            channels = BroadcastChannelMapper.MapToViewModels(entities, entities.Select(e => e.ChannelId).ToList());
            _memoryCache.Set(cacheKey, channels, TimeSpan.FromMinutes(15));
        }

        return channels;
    }

    public async Task<Dictionary<string, List<BroadcastChannelViewModel>>> GetUserFavoriteChannelsGroupedByCountryAsync(int userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"user_{userId}_favorite_channels_grouped";
        var groupedChannels = _memoryCache.Get<Dictionary<string, List<BroadcastChannelViewModel>>>(cacheKey);

        if (groupedChannels == null)
        {
            groupedChannels = new Dictionary<string, List<BroadcastChannelViewModel>>();

            // Get user's favorite channels
            var favoriteChannels = await GetUserFavoriteChannelsAsync(userId, cancellationToken);
            var favoriteChannelIds = favoriteChannels.Select(c => c.ChannelId).ToHashSet();

            // Get all countries
            var countries = await _broadcastChannelRepository.GetAllCountriesAsync(cancellationToken);

            // For each country, get its channels that are in user's favorites
            foreach (var country in countries.OrderBy(c => c.CountryName))
            {
                var countryChannels = await GetChannelsByCountryCodeAsync(country.CountryCode, cancellationToken);
                var favoriteCountryChannels = countryChannels
                    .Where(c => favoriteChannelIds.Contains(c.ChannelId))
                    .Select(c => new BroadcastChannelViewModel
                    {
                        ChannelId = c.ChannelId,
                        Name = c.Name,
                        Code = c.Code,
                        IsFavorite = true
                    })
                    .ToList();

                if (favoriteCountryChannels.Count > 0)
                {
                    groupedChannels[country.CountryName] = favoriteCountryChannels;
                }
            }

            _memoryCache.Set(cacheKey, groupedChannels, TimeSpan.FromMinutes(15));
        }

        return groupedChannels;
    }

    public async Task<bool> IsFavoriteChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default)
    {
        return await _broadcastChannelRepository.IsFavoriteChannelAsync(userId, channelId, cancellationToken);
    }

    public async Task AddUserFavoriteChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default)
    {
        await _broadcastChannelRepository.AddUserFavoriteChannelAsync(userId, channelId, cancellationToken);

        // Invalidate cache
        _memoryCache.Remove($"user_{userId}_favorite_channel_ids");
        _memoryCache.Remove($"user_{userId}_favorite_channels");
        _memoryCache.Remove($"user_{userId}_favorite_channels_grouped");
    }

    public async Task RemoveUserFavoriteChannelAsync(int userId, int channelId, CancellationToken cancellationToken = default)
    {
        await _broadcastChannelRepository.RemoveUserFavoriteChannelAsync(userId, channelId, cancellationToken);

        // Invalidate cache
        _memoryCache.Remove($"user_{userId}_favorite_channel_ids");
        _memoryCache.Remove($"user_{userId}_favorite_channels");
        _memoryCache.Remove($"user_{userId}_favorite_channels_grouped");
    }

    // ==================== TV Listings ====================

    public async Task<TvListingsViewModel> GetTvListingsAsync(DateTime startDate, DateTime endDate, int? userId = null, CancellationToken cancellationToken = default)
    {
        // Get upcoming matches in date range
        var matches = await _matchesRepository.GetAllUnfinishedMatchesAsync();
        var filteredMatches = matches
            .Where(m => m.MatchDateUTC >= startDate && m.MatchDateUTC < endDate)
            .OrderBy(m => m.MatchDateUTC)
            .ToList();

        if (filteredMatches.Count == 0)
            return new TvListingsViewModel { StartDate = startDate, EndDate = endDate };

        // Get match IDs
        var matchIds = filteredMatches.Select(m => m.MatchId).ToList();

        // Get broadcasts for all matches
        var broadcastsDict = await GetBroadcastsByMatchIdsAsync(matchIds, cancellationToken);

        // If user is logged in and has favorite channels, filter broadcasts
        List<int>? favoriteChannelIds = null;
        if (userId.HasValue)
        {
            favoriteChannelIds = await GetUserFavoriteChannelIdsAsync(userId.Value, cancellationToken);
        }

        // Build TV listings view model
        var tvListings = new TvListingsViewModel
        {
            StartDate = startDate,
            EndDate = endDate
        };

        // Group matches by day
        var matchesByDay = filteredMatches.GroupBy(m => m.MatchDateUTC.Date);

        foreach (var dayGroup in matchesByDay)
        {
            var dayViewModel = new TvListingDayViewModel
            {
                Date = dayGroup.Key
            };

            // Group matches by hour time slot
            var matchesByHour = dayGroup.GroupBy(m => new DateTime(m.MatchDateUTC.Year, m.MatchDateUTC.Month, m.MatchDateUTC.Day, m.MatchDateUTC.Hour, 0, 0));

            foreach (var hourGroup in matchesByHour.OrderBy(g => g.Key))
            {
                var timeSlotViewModel = new TvListingTimeSlotViewModel
                {
                    TimeSlot = hourGroup.Key
                };

                foreach (var match in hourGroup)
                {
                    // Get broadcasts for this match
                    var matchBroadcasts = broadcastsDict.ContainsKey(match.MatchId)
                        ? broadcastsDict[match.MatchId]
                        : new List<MatchBroadcastViewModel>();

                    // Filter by favorite channels if user is logged in
                    if (favoriteChannelIds != null && favoriteChannelIds.Count > 0)
                    {
                        matchBroadcasts = matchBroadcasts
                            .Where(b => favoriteChannelIds.Contains(b.ChannelId))
                            .ToList();
                    }

                    // Only add match if it has broadcasts (or if no user filter)
                    if (matchBroadcasts.Count > 0 || favoriteChannelIds == null)
                    {
                        var matchViewModel = new TvListingMatchViewModel
                        {
                            MatchId = match.MatchId,
                            CompetitionId = match.CompetitionId,
                            CompetitionName = match.CompetitionName,
                            CompetitionBackgroundColor = match.CompetitionBackgroundColor,
                            MatchDateUTC = match.MatchDateUTC.DateTime,
                            HomeTeamId = match.HomeTeamId,
                            HomeTeamName = match.HomeTeamName,
                            AwayTeamId = match.AwayTeamId,
                            AwayTeamName = match.AwayTeamName,
                            ExcitmentScore = match.ExcitmentScore,
                            Broadcasts = matchBroadcasts
                        };

                        timeSlotViewModel.Matches.Add(matchViewModel);
                    }
                }

                // Only add time slot if it has matches
                if (timeSlotViewModel.Matches.Count > 0)
                {
                    dayViewModel.TimeSlots.Add(timeSlotViewModel);
                }
            }

            // Only add day if it has time slots
            if (dayViewModel.TimeSlots.Count > 0)
            {
                tvListings.Days.Add(dayViewModel);
            }
        }

        return tvListings;
    }
}
