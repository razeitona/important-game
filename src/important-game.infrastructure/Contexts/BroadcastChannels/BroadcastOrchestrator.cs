
using important_game.infrastructure.Contexts.BroadcastChannels.Data;
using important_game.infrastructure.Contexts.BroadcastChannels.Data.Entities;
using important_game.infrastructure.Contexts.BroadcastChannels.Models;
using important_game.infrastructure.Contexts.Matches.Data;

namespace important_game.infrastructure.Contexts.BroadcastChannels;
public interface IBroadcastOrchestrator
{
    Task ProcessBroadcastGuide();
}

internal class BroadcastOrchestrator(ITvGuideMatcher tvGuideMatcher, IMatchesRepository matchRepository, IBroadcastChannelRepository broadcastRepository) : IBroadcastOrchestrator
{
    public async Task ProcessBroadcastGuide()
    {
        var countries = await broadcastRepository.GetAllCountriesAsync();
        if (countries.Count == 0)
            return;
        var allFutureMatches = await matchRepository.GetMatchesToBroadcastInTimeRangeAsync(DateTime.UtcNow.Date);
        if (allFutureMatches.Count == 0)
            return;

        foreach (var country in countries)
        {
            var fileName = $"guide.{country.CountryCode.ToLower()}.xml";
            if (!File.Exists(fileName))
                continue;

            var guideXML = await File.ReadAllTextAsync(fileName);
            var matches = tvGuideMatcher.ProcessGuideAndFindMatchesAsync(guideXML, allFutureMatches);

            await PersistMatchesBroadcastsAsync(country, matches);
        }
    }

    private async Task PersistMatchesBroadcastsAsync(CountryEntity country, List<BroadcastMatchLinkDto> matches)
    {
        var groupedBroadcasts = matches.GroupBy(c => c.MappedChannelCode).ToList();

        var matchBroadCasts = new List<MatchBroadcastEntity>();
        foreach (var broadcast in groupedBroadcasts)
        {
            var firstBroadcast = broadcast.First();
            var broadcastEntity = new BroadcastChannelEntity
            {
                Name = firstBroadcast.ChannelName,
                Code = firstBroadcast.MappedChannelCode,
                CountryCode = country.CountryCode
            };

            broadcastEntity = await broadcastRepository.UpsertBroadcastChannelAsync(broadcastEntity);

            foreach (var broadcastMatch in broadcast)
            {
                var matchBroadcastEntity = new MatchBroadcastEntity
                {
                    MatchId = broadcastMatch.MatchId,
                    ChannelId = broadcastEntity.ChannelId,
                };
                matchBroadCasts.Add(matchBroadcastEntity);
            }

        }

        await broadcastRepository.UpsertBroadcastMatchAsync(matchBroadCasts);
    }
}
