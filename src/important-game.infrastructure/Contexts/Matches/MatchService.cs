using important_game.infrastructure.Contexts.BroadcastChannels;
using important_game.infrastructure.Contexts.Competitions.Data;
using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Data;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Mappers;
using important_game.infrastructure.Contexts.Matches.Models;
using important_game.infrastructure.Contexts.Teams.Data;
using Microsoft.Extensions.Caching.Memory;

namespace important_game.infrastructure.Contexts.Matches;
internal class MatchService(
    IMatchesRepository matchesRepository,
    ICompetitionRepository competitionRepository,
    ITeamRepository teamRepository,
    IBroadcastChannelService broadcastChannelService,
    IMemoryCache memoryCache) : IMatchService
{
    public async Task<List<MatchDto>> GetAllUnfinishedMatchesAsync(CancellationToken cancellationToken = default)
    {
        var upcomingMatches = memoryCache.Get<List<MatchDto>>("upcoming_matches");
        if (upcomingMatches == null)
        {
            upcomingMatches = await matchesRepository.GetAllUnfinishedMatchesAsync();
            if (upcomingMatches != null && upcomingMatches.Count > 0)
                memoryCache.Set("upcoming_matches", upcomingMatches, TimeSpan.FromMinutes(5));
        }
        return upcomingMatches?.OrderBy(c => c.MatchDateUTC).ToList() ?? [];
    }

    public async Task<List<MatchDto>> GetUserFavoriteUpcomingMatchesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var userFavoriteMatches = await matchesRepository.GetUserFavoriteUpcomingMatchesAsync(userId).ConfigureAwait(false);
        return userFavoriteMatches;
    }

    public async Task<MatchesViewModel> GetAllMatchesAsync(CancellationToken cancellationToken = default)
    {
        var upcomingMatches = await GetAllUnfinishedMatchesAsync(cancellationToken);

        if (upcomingMatches == null || upcomingMatches.Count == 0)
            return new();

        DateTime now = DateTime.UtcNow;
        upcomingMatches = upcomingMatches.Where(c => c.MatchDateUTC > now.AddMinutes(-120)).ToList();

        // Get broadcasts for all matches
        var matchIds = upcomingMatches.Select(m => m.MatchId).ToList();
        var broadcastsDict = await broadcastChannelService.GetBroadcastsByMatchIdsAsync(matchIds, cancellationToken);

        // Assign broadcasts to matches
        foreach (var match in upcomingMatches)
        {
            if (broadcastsDict.ContainsKey(match.MatchId))
            {
                match.Broadcasts = broadcastsDict[match.MatchId];
            }
        }

        var matchViewModel = new MatchesViewModel();
        matchViewModel.Matches = upcomingMatches.OrderBy(c => c.MatchDateUTC).ToList();

        var groupCompetitions = upcomingMatches.GroupBy(c => c.CompetitionId);
        foreach (var group in groupCompetitions)
        {
            var competitionId = group.Key;
            var rawCompetition = group.First();

            var competition = new CompetitionDto()
            {
                CompetitionId = competitionId,
                Name = rawCompetition.CompetitionName,
                PrimaryColor = rawCompetition.CompetitionPrimaryColor,
                BackgroundColor = rawCompetition.CompetitionBackgroundColor
            };

            matchViewModel.Competitions.Add(competition);
        }


        return matchViewModel;
    }

    public async Task<MatchDetailViewModel?> GetMatchByIdAsync(int matchId, CancellationToken cancellationToken = default)
    {

        var matchDetailDto = memoryCache.Get<MatchDetailDto>($"match_{matchId}");
        if (matchDetailDto == null)
        {
            matchDetailDto = await matchesRepository.GetMatchByIdAsync(matchId);
            if (matchDetailDto != null)
                memoryCache.Set($"match_{matchId}", matchDetailDto, TimeSpan.FromMinutes(5));
        }

        if (matchDetailDto == null)
            return default;

        var matchDetail = MatchMapper.MapToMatchDetail(matchDetailDto);

        // Setup title holder information
        if (matchDetailDto.SeasonId != null)
        {
            var seasonInfo = await competitionRepository.GetCompetitionSeasonByIdAsync(matchDetailDto.SeasonId.Value);
            if (seasonInfo != null && seasonInfo.TitleHolderId.HasValue)
            {
                matchDetail.TitleHolderId = seasonInfo.TitleHolderId;
                matchDetail.HasTitleHolder = seasonInfo.TitleHolderId.Value == matchDetail.HomeTeamId || seasonInfo.TitleHolderId.Value == matchDetail.AwayTeamId;
            }
        }

        // Setup head to head information
        var headToHeadMatches = await matchesRepository.GetHeadToHeadMatchesAsync(matchDetail.HomeTeamId, matchDetail.AwayTeamId);
        matchDetail.HeadToHead = headToHeadMatches;

        // Setup rivalry information
        var rivalryInformation = await matchesRepository.GetRivalryAsync(matchDetail.HomeTeamId, matchDetail.AwayTeamId);
        matchDetail.IsRivalry = rivalryInformation != null;

        // Setup league table information
        await PopulateLeagueTableAsync(matchDetail);

        // Setup broadcast channels information
        matchDetail.BroadcastsByCountry = await broadcastChannelService.GetBroadcastsByMatchIdGroupedByCountryAsync(matchId, cancellationToken);

        return matchDetail;
    }

    public async Task<MatchDetailViewModel?> GetMatchByTeamSlugsAsync(string homeSlug, string awaySlug, CancellationToken cancellationToken = default)
    {
        var matchDetailDto = await matchesRepository.GetMatchByTeamSlugsAsync(homeSlug, awaySlug);
        if (matchDetailDto == null)
            return default;

        var matchDetail = MatchMapper.MapToMatchDetail(matchDetailDto);

        // Setup title holder information
        if (matchDetailDto.SeasonId != null)
        {
            var seasonInfo = await competitionRepository.GetCompetitionSeasonByIdAsync(matchDetailDto.SeasonId.Value);
            if (seasonInfo != null && seasonInfo.TitleHolderId.HasValue)
            {
                matchDetail.TitleHolderId = seasonInfo.TitleHolderId;
                matchDetail.HasTitleHolder = seasonInfo.TitleHolderId.Value == matchDetail.HomeTeamId || seasonInfo.TitleHolderId.Value == matchDetail.AwayTeamId;
            }
        }

        // Setup head to head information
        var headToHeadMatches = await matchesRepository.GetHeadToHeadMatchesAsync(matchDetail.HomeTeamId, matchDetail.AwayTeamId);
        matchDetail.HeadToHead = headToHeadMatches;

        // Setup rivalry information
        var rivalryInformation = await matchesRepository.GetRivalryAsync(matchDetail.HomeTeamId, matchDetail.AwayTeamId);
        matchDetail.IsRivalry = rivalryInformation != null;

        // Setup league table information
        await PopulateLeagueTableAsync(matchDetail);

        // Setup broadcast channels information
        matchDetail.BroadcastsByCountry = await broadcastChannelService.GetBroadcastsByMatchIdGroupedByCountryAsync(matchDetail.MatchId, cancellationToken);

        return matchDetail;
    }

    public async Task<MatchDetailViewModel?> GetMatchOfTheDayAsync(CancellationToken cancellationToken = default)
    {
        var matchDetailDto = memoryCache.Get<MatchDetailDto>("match_of_the_day");
        if (matchDetailDto == null)
        {
            matchDetailDto = await matchesRepository.GetMatchOfTheDayAsync();
            if (matchDetailDto != null)
                memoryCache.Set("match_of_the_day", matchDetailDto, TimeSpan.FromMinutes(2));
        }

        if (matchDetailDto == null)
            return default;

        var matchDetail = MatchMapper.MapToMatchDetail(matchDetailDto);

        return matchDetail;
    }

    private async Task PopulateLeagueTableAsync(MatchDetailViewModel matchDetail)
    {
        if (matchDetail.SeasonId == null)
            return;

        var leagueTableEntities = await competitionRepository.GetCompetitionTableAsync(matchDetail.CompetitionId, matchDetail.SeasonId.Value);
        if (leagueTableEntities == null || leagueTableEntities.Count == 0)
            return;

        var teamIds = leagueTableEntities.Select(t => t.TeamId).Distinct().ToList();
        var teams = await teamRepository.GetTeamsByIdsAsync(teamIds);

        matchDetail.LeagueTable = leagueTableEntities
            .OrderBy(t => t.Position)
            .Select(t =>
            {
                var team = teams.FirstOrDefault(tm => tm.Id == t.TeamId);
                return new LeagueTableRowDto
                {
                    TeamId = t.TeamId,
                    TeamName = team?.Name ?? "Unknown",
                    Position = t.Position,
                    Matches = t.Matches,
                    Wins = t.Wins,
                    Draws = t.Draws,
                    Losses = t.Losses,
                    GoalsFor = t.GoalsFor,
                    GoalsAgainst = t.GoalsAgainst,
                    Points = t.Points,
                    IsPlayingTeam = t.TeamId == matchDetail.HomeTeamId || t.TeamId == matchDetail.AwayTeamId
                };
            })
            .ToList();
    }
}
