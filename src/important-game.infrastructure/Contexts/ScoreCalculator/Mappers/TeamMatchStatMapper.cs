using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.ScoreCalculator.Models;
using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.infrastructure.Contexts.ScoreCalculator.Mappers;
internal static class TeamMatchStatMapper
{
    public static TeamMatchesStatsDto MapToTeamMatchStatDto(int teamId, List<MatchesEntity> matches, List<HeadToHeadDto> headToHeadMatches)
    {
        var statsDto = new TeamMatchesStatsDto();
        statsDto.Matches = matches.Count;
        PrepareMatchesStats(teamId, matches, statsDto);
        PrepareHeadToHeadStats(teamId, headToHeadMatches, statsDto);

        return statsDto;
    }

    private static void PrepareMatchesStats(int teamId, List<MatchesEntity> matches, TeamMatchesStatsDto statsDto)
    {
        foreach (var match in matches)
        {
            if (match.HomeScore == match.AwayScore)
            {
                statsDto.Draws++;
                statsDto.Form.Add(MatchResultType.Draw);
            }
            else if ((match.HomeTeamId == teamId && match.HomeScore > match.AwayScore) ||
                     (match.AwayTeamId == teamId && match.AwayScore > match.HomeScore))
            {
                statsDto.Wins++;
                statsDto.GoalsFor += match.HomeTeamId == teamId ? match.HomeScore ?? 0 : match.AwayScore ?? 0;
                statsDto.GoalsAgainst += match.HomeTeamId != teamId ? match.HomeScore ?? 0 : match.AwayScore ?? 0;
                statsDto.Form.Add(MatchResultType.Win);
            }
            else
            {
                statsDto.Losses++;
                statsDto.GoalsFor += match.HomeTeamId == teamId ? match.HomeScore ?? 0 : match.AwayScore ?? 0;
                statsDto.GoalsAgainst += match.HomeTeamId != teamId ? match.HomeScore ?? 0 : match.AwayScore ?? 0;
                statsDto.Form.Add(MatchResultType.Lost);
            }
        }
    }

    private static void PrepareHeadToHeadStats(int teamId, List<HeadToHeadDto> headToHeadMatches, TeamMatchesStatsDto statsDto)
    {
        var matches = headToHeadMatches.Where(c => c.MatchDateUTC > DateTimeOffset.UtcNow.AddYears(-2)).OrderByDescending(h => h.MatchDateUTC).Take(5).ToList();
        statsDto.HeadToHeadMatches = matches.Count;

        foreach (var match in matches)
        {
            if (match.HomeTeamScore == match.AwayTeamScore)
            {
                statsDto.HeadToHeadDraws++;
            }
            else if ((match.HomeTeamId == teamId && match.HomeTeamScore > match.AwayTeamScore) ||
                     (match.AwayTeamId == teamId && match.AwayTeamScore > match.HomeTeamScore))
            {
                statsDto.HeadToHeadWins++;
                statsDto.HeadToHeadGoalsFor += match.HomeTeamId == teamId ? match.HomeTeamScore : match.AwayTeamScore;
                statsDto.HeadToHeadGoalsAgainst += match.HomeTeamId != teamId ? match.HomeTeamScore : match.AwayTeamScore;
            }
            else
            {
                statsDto.HeadToHeadLosses++;
                statsDto.HeadToHeadGoalsFor += match.HomeTeamId == teamId ? match.HomeTeamScore : match.AwayTeamScore;
                statsDto.HeadToHeadGoalsAgainst += match.HomeTeamId != teamId ? match.HomeTeamScore : match.AwayTeamScore;
            }
        }
    }
}
