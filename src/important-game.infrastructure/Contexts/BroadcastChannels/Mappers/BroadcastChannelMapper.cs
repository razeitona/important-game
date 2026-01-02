using important_game.infrastructure.Contexts.BroadcastChannels.Data.Entities;
using important_game.infrastructure.Contexts.BroadcastChannels.Models;

namespace important_game.infrastructure.Contexts.BroadcastChannels.Mappers;

public static class BroadcastChannelMapper
{
    public static CountryViewModel MapToCountryViewModel(CountryEntity entity)
    {
        return new CountryViewModel
        {
            CountryCode = entity.CountryCode,
            CountryName = entity.CountryName
        };
    }

    public static BroadcastChannelViewModel MapToViewModel(BroadcastChannelEntity entity, bool isFavorite = false)
    {
        return new BroadcastChannelViewModel
        {
            ChannelId = entity.ChannelId,
            Name = entity.Name,
            Code = entity.Code,
            IsFavorite = isFavorite
        };
    }

    public static List<BroadcastChannelViewModel> MapToViewModels(List<BroadcastChannelEntity> entities, List<int>? favoriteChannelIds = null)
    {
        var favoriteSet = favoriteChannelIds?.ToHashSet() ?? new HashSet<int>();
        return entities.Select(e => MapToViewModel(e, favoriteSet.Contains(e.ChannelId))).ToList();
    }

    public static MatchBroadcastViewModel MapToMatchBroadcastViewModel(MatchBroadcastDto dto)
    {
        return new MatchBroadcastViewModel
        {
            MatchId = dto.MatchId,
            ChannelId = dto.ChannelId,
            ChannelName = dto.ChannelName,
            ChannelCode = dto.ChannelCode,
            CountryCode = dto.CountryCode,
            CountryName = dto.CountryName
        };
    }

    public static List<MatchBroadcastViewModel> MapToMatchBroadcastViewModels(List<MatchBroadcastDto> dtos)
    {
        if (dtos == null || dtos.Count == 0)
            return [];

        var result = new List<MatchBroadcastViewModel>();
        foreach (var match in dtos)
        {
            var viewModel = MapToMatchBroadcastViewModel(match);
            result.Add(viewModel);
        }
        return result;
    }

    public static Dictionary<string, List<MatchBroadcastViewModel>> GroupBroadcastsByCountry(List<MatchBroadcastViewModel> broadcasts)
    {
        return broadcasts
            .GroupBy(b => b.CountryName)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.OrderBy(b => b.ChannelName).ToList());
    }
}
