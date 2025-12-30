using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.ScoreCalculator.Mappers;
using important_game.infrastructure.Contexts.ScoreCalculator.Models;
using important_game.infrastructure.Contexts.ScoreCalculator.Utils;
using Microsoft.Extensions.Logging;

namespace important_game.infrastructure.Contexts.ScoreCalculator;

public interface IMatchCalculator
{
    MatchCalcsDto? CalculateMatchScore(UnfinishedMatchDto match, List<CompetitionTableEntity> competitionTable, RivalryEntity? rivarlyInformation
        , List<HeadToHeadDto> headToHeadMatches, List<MatchesEntity> homeMatches, List<MatchesEntity> awayMatches);
}

public class MatchCalculator(ILogger<MatchCalculator> logger) : IMatchCalculator
{
    public MatchCalcsDto? CalculateMatchScore(UnfinishedMatchDto match, List<CompetitionTableEntity> competitionTable,
        RivalryEntity? rivarlyInformation, List<HeadToHeadDto> headToHeadMatches, List<MatchesEntity> homeMatches, List<MatchesEntity> awayMatches)
    {

        var homeTeamTable = competitionTable.FirstOrDefault(c => c.TeamId == match.HomeTeamId);
        var awayTeamTable = competitionTable.FirstOrDefault(c => c.TeamId == match.AwayTeamId);

        if (homeTeamTable == null || awayTeamTable == null)
        {
            logger.LogWarning("Could not find competition table data for match {MatchId}", match.MatchId);
            return default;
        }

        var homeTeamMatchesStats = TeamMatchStatMapper.MapToTeamMatchStatDto(match.HomeTeamId, homeMatches, headToHeadMatches);
        var awayTeamMatchesStats = TeamMatchStatMapper.MapToTeamMatchStatDto(match.AwayTeamId, awayMatches, headToHeadMatches);

        var matchResult = new MatchCalcsDto();
        matchResult.MatchId = match.MatchId;
        matchResult.CompetitionScore = match.LeagueRanking;
        matchResult.CompetitionStandingScore = CalculateLeagueTableValue(homeTeamTable, awayTeamTable, match.NumberOfRounds);
        matchResult.FixtureScore = CalculateFixtureValue(match.Round, match.NumberOfRounds);
        matchResult.FormScore = CalculateFormScore(homeTeamMatchesStats, awayTeamMatchesStats);
        matchResult.GoalsScore = CalculateGoalScore(homeTeamMatchesStats, awayTeamMatchesStats);
        matchResult.HeadToHeadScore = CalculateHeadToHeadScore(homeTeamMatchesStats, awayTeamMatchesStats);
        matchResult.RivalryScore = rivarlyInformation?.RivarlyValue ?? 0d;
        matchResult.TitleHolderScore = CalculateTitleHolderScore(match.HomeTeamId, match.AwayTeamId, match.TitleHolderId);

        var isLateStage = IsLateStage(homeTeamTable.Position, match.NumberOfRounds, competitionTable.Count);
        matchResult.ExcitmentScore = CalculateExcitmentScore(matchResult, isLateStage);

        matchResult.HomeForm = string.Join(",", homeTeamMatchesStats.Form.Select(c => (int)c).ToArray());
        matchResult.AwayForm = string.Join(",", awayTeamMatchesStats.Form.Select(c => (int)c).ToArray());
        matchResult.HomeTeamPosition = homeTeamTable.Position;
        matchResult.AwayTeamPosition = awayTeamTable.Position;

        return matchResult;
    }

    private static double CalculateGoalScore(TeamMatchesStatsDto homeStats, TeamMatchesStatsDto awayStats)
    {
        var homeTeamGoalScore = homeStats.Matches == 0 ? 0d : (double)homeStats.GoalsFor / ((double)homeStats.Matches * 2.0d);
        var awayTeamGoalScore = awayStats.Matches == 0 ? 0d : (double)awayStats.GoalsFor / ((double)awayStats.Matches * 2.0d);
        return Math.Min(Math.Round((homeTeamGoalScore + awayTeamGoalScore)/2.0d, 3),1.0d);
    }

    private static double CalculateFixtureValue(int? currentRound, int? totalRounds)
    {
        if (!currentRound.HasValue || !totalRounds.HasValue || totalRounds == 0)
            return 0d;

        return Math.Round((double)currentRound / (double)totalRounds, 3);
    }

    private static double CalculateFormScore(TeamMatchesStatsDto homeStats, TeamMatchesStatsDto awayStats)
    {
        var homeTeamFormScore = homeStats.Matches == 0 ? 0d : ((double)(homeStats.Wins * 3d) + (double)homeStats.Draws) / 15d;
        var awayTeamFormScore = awayStats.Matches == 0 ? 0d : ((double)(awayStats.Wins * 3d) + (double)awayStats.Draws) / 15d;
        return Math.Round((homeTeamFormScore + awayTeamFormScore) / 2d, 3);
    }

    private static double CalculateLeagueTableValue(CompetitionTableEntity homeTeamTable, CompetitionTableEntity awayTeamTable, int totalRounds)
    {
        if (totalRounds == 0)
            return 0d;

        if (totalRounds <= 1)
            return 0.5d;

        double totalTeams = ((double)totalRounds / 2) - 1;

        var homeTeamPosition = (double)homeTeamTable.Position;
        var awayTeamPosition = (double)awayTeamTable.Position;

        var positionDiff = Math.Abs(homeTeamPosition - awayTeamPosition) - 1d;
        var totalTeamsDouble = (double)totalTeams;

        var positionValue = 1d / (1d + (positionDiff / (totalTeamsDouble - 1d)));
        var averageTeamPosition = (homeTeamPosition + awayTeamPosition) / 2d;
        var topBottomMatchupValue = 1d - (averageTeamPosition - 1d) / (totalTeamsDouble - 1d);

        var homeTeamPoints = (double)homeTeamTable.Points;
        var awayTeamPoints = (double)awayTeamTable.Points;

        var homeTeamMatches = (double)homeTeamTable.Matches;
        var awayTeamMatches = (double)awayTeamTable.Matches;

        var pointDifference = Math.Abs(homeTeamPoints - awayTeamPoints);
        var maxPointDifference = (totalRounds - Math.Max(homeTeamMatches, awayTeamMatches)) * 3d;

        if (maxPointDifference <= 0)
            return Math.Round(positionValue * topBottomMatchupValue, 3);

        var pointImpactValue = 1d / (1d + (pointDifference / (maxPointDifference - 1d)));

        return Math.Round(positionValue * topBottomMatchupValue * pointImpactValue, 3);
    }

    public static double CalculateTitleHolderScore(int homeTeamId, int awayTeamId, int? titleHolderId)
    {
        if (!titleHolderId.HasValue)
            return 0d;

        return Math.Round(homeTeamId == titleHolderId.Value || awayTeamId == titleHolderId.Value ? 1d : 0d, 3);
    }

    private static double CalculateHeadToHeadScore(TeamMatchesStatsDto homeStats, TeamMatchesStatsDto awayStats)
    {
        if (homeStats.HeadToHeadMatches == 0)
            return 0.5d;

        // Maximum value from 5 games: 3+3+3 (3 wins each + 2 draws) = 15
        var value = ((homeStats.HeadToHeadWins + awayStats.HeadToHeadWins) * 3d + homeStats.HeadToHeadDraws) / 15d;
        return Math.Round(value > 1d ? 1d : value, 3);
    }

    public static bool IsLateStage(int currentRound, int? totalRounds, int totalStandings)
    {
        if (!totalRounds.HasValue || totalRounds == 0)
            return false;

        var stagePercentage = (double)currentRound / (double)totalRounds;
        return stagePercentage > 0.8d && totalRounds > totalStandings;
    }

    private static double CalculateExcitmentScore(MatchCalcsDto matchResult, bool isLateStage)
    {
        double competitionScore = matchResult.CompetitionScore * CalculatorCoeficients.CompetitionCoef(isLateStage);
        double fixtureScore = matchResult.FixtureScore * CalculatorCoeficients.FixtureCoef(isLateStage);
        double formScore = matchResult.FormScore * CalculatorCoeficients.TeamFormCoef(isLateStage);
        double goalsScore = matchResult.GoalsScore * CalculatorCoeficients.TeamGoalsCoef(isLateStage);
        double standingsScore = matchResult.CompetitionStandingScore * CalculatorCoeficients.TableRankCoef(isLateStage);
        double headToheadScore = matchResult.HeadToHeadScore * CalculatorCoeficients.HeadToHeadCoef(isLateStage);
        double titleScore = matchResult.TitleHolderScore * CalculatorCoeficients.TitleHolderCoef(isLateStage);
        double rivalryScore = matchResult.RivalryScore * CalculatorCoeficients.RivalryCoef(isLateStage);

        var finalScore = competitionScore + fixtureScore + formScore + goalsScore +
                         standingsScore + headToheadScore + titleScore + rivalryScore;
        return Math.Round(finalScore, 3);

    }
}
