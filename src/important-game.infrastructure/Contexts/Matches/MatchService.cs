using important_game.infrastructure.Contexts.Competitions.Data;
using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Data;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Mappers;
using important_game.infrastructure.Contexts.Matches.Models;

namespace important_game.infrastructure.Contexts.Matches;
internal class MatchService(IMatchesRepository matchesRepository, ICompetitionRepository competitionRepository) : IMatchService
{
    public async Task<List<MatchDto>> GetAllUpcomingMatchesAsync(CancellationToken cancellationToken = default)
    {
        var upcomingMatches = await matchesRepository.GetAllUpcomingMatchesAsync();
        return upcomingMatches?.OrderBy(c => c.MatchDateUTC).ToList() ?? [];
    }

    public async Task<MatchesViewModel> GetAllMatchesAsync(CancellationToken cancellationToken = default)
    {
        var upcomingMatches = await matchesRepository.GetAllUpcomingMatchesAsync();

        if (upcomingMatches == null || upcomingMatches.Count == 0)
            return new();

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
        var matchDetailDto = await matchesRepository.GetMatchByIdAsync(matchId);
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

        return matchDetail;
    }
}
