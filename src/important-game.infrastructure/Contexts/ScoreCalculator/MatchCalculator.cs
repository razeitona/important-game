using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.ScoreCalculator.Utils;
using Microsoft.Extensions.Logging;

namespace important_game.infrastructure.Contexts.ScoreCalculator;
public class MatchCalculator(ILogger<MatchCalculator> logger) : IMatchCalculator
{
    public MatchCalcsDto? CalculateMatchScore(UnfinishedMatchDto match, List<CompetitionTableEntity> competitionTable,
        RivalryEntity? rivarlyInformation, List<HeadToHeadDto> headToHeadMatches)
    {

        var homeTeamTable = competitionTable.FirstOrDefault(c => c.TeamId == match.HomeTeamId);
        var awayTeamTable = competitionTable.FirstOrDefault(c => c.TeamId == match.AwayTeamId);

        if (homeTeamTable == null || awayTeamTable == null)
        {
            logger.LogWarning("Could not find competition table data for match {MatchId}", match.MatchId);
            return default;
        }

        var matchResult = new MatchCalcsDto();
        matchResult.MatchId = match.MatchId;

        matchResult.CompetitionScore = match.LeagueRanking;
        matchResult.FixtureScore = CalculateFixtureValue(match.Round, match.NumberOfRounds);

        var homeTeamFormScore = CalculateTeamFormScore(homeTeamTable.Wins, homeTeamTable.Draws, homeTeamTable.Matches);
        var awayTeamFormScore = CalculateTeamFormScore(awayTeamTable.Wins, awayTeamTable.Draws, awayTeamTable.Matches);
        matchResult.FormScore = (homeTeamFormScore + awayTeamFormScore) / 2d;

        var homeTeamGoalScore = CalculateTeamGoalsScore(homeTeamTable.GoalsFor, homeTeamTable.Matches);
        var awayTeamGoalScore = CalculateTeamGoalsScore(awayTeamTable.GoalsFor, awayTeamTable.Matches);
        matchResult.GoalsScore = homeTeamGoalScore + awayTeamGoalScore;

        matchResult.CompetitionStandingScore = CalculateLeagueTableValue(homeTeamTable, awayTeamTable, match.NumberOfRounds);
        matchResult.HeadToHeadScore = CalculateHeadToHeadScore(headToHeadMatches, match.HomeTeamId, match.AwayTeamId);
        matchResult.TitleHolderScore = CalculateTitleHolderScore(match.HomeTeamId, match.AwayTeamId, match.TitleHolderId);
        matchResult.RivalryScore = rivarlyInformation?.RivarlyValue ?? 0d;

        var isLateStage = IsLateStage(homeTeamTable.Position, match.NumberOfRounds, competitionTable.Count);

        matchResult.ExcitmentScore =
            matchResult.CompetitionScore * CalculatorCoeficients.CompetitionCoef +
            matchResult.FixtureScore * CalculatorCoeficients.FixtureCoef +
            matchResult.FormScore * CalculatorCoeficients.TeamFormCoef +
            matchResult.GoalsScore * CalculatorCoeficients.TeamGoalsCoef +
            matchResult.CompetitionStandingScore * CalculatorCoeficients.TableRankCoef +
            matchResult.HeadToHeadScore * CalculatorCoeficients.HeadToHeadCoef +
            matchResult.TitleHolderScore * CalculatorCoeficients.TitleHolderCoef +
            matchResult.RivalryScore * CalculatorCoeficients.RivalryCoef;

        return matchResult;


    }

    private double CalculateFixtureValue(int? currentRound, int? totalRounds)
    {
        if (!currentRound.HasValue || !totalRounds.HasValue || totalRounds == 0)
            return 0d;

        return (double)currentRound / (double)totalRounds;
    }

    private double CalculateLeagueTableValue(CompetitionTableEntity homeTeamTable, CompetitionTableEntity awayTeamTable, int totalRounds)
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
            return positionValue * topBottomMatchupValue;

        var pointImpactValue = 1d / (1d + (pointDifference / (maxPointDifference - 1d)));

        return positionValue * topBottomMatchupValue * pointImpactValue;
    }

    public double CalculateTitleHolderScore(int homeTeamId, int awayTeamId, int? titleHolderId)
    {
        if (!titleHolderId.HasValue)
            return 0d;

        return homeTeamId == titleHolderId.Value || awayTeamId == titleHolderId.Value ? 1d : 0d;
    }

    public double CalculateTeamFormScore(int wins, int draws, int matches)
    {
        if (matches == 0)
            return 0d;

        // Maximum points in last 5 games is 15 (5 wins)
        return ((double)(wins * 3) + (double)draws) / 15d;
    }

    public double CalculateTeamGoalsScore(int goalsFor, int matches)
    {
        if (matches == 0)
            return 0d;

        return (double)goalsFor / (double)matches * 2.0;
    }

    private double? CalculateHeadToHeadScore(List<HeadToHeadDto> headToHeadMatches, int homeTeamId, int awayTeamId)
    {
        if (headToHeadMatches == null || headToHeadMatches.Count == 0)
            return 0.5d;

        var matches = headToHeadMatches.Where(c => c.MatchDateUTC > DateTimeOffset.UtcNow.AddYears(-2)).OrderByDescending(h => h.MatchDateUTC).Take(5);
        int homeTeamWins = 0, awayTeamWins = 0, draws = 0;

        foreach (var match in matches)
        {
            if (match.HomeTeamId == homeTeamId)
            {
                if (match.HomeTeamScore > match.AwayTeamScore)
                    homeTeamWins++;
                else if (match.HomeTeamScore < match.AwayTeamScore)
                    awayTeamWins++;
                else
                    draws++;
            }
            else if (match.AwayTeamId == homeTeamId)
            {
                if (match.AwayTeamScore > match.HomeTeamScore)
                    homeTeamWins++;
                else if (match.AwayTeamScore < match.HomeTeamScore)
                    awayTeamWins++;
                else
                    draws++;
            }
        }

        if (homeTeamWins == 0 && awayTeamWins == 0 && draws == 0)
            return 0.5d;

        // Maximum value from 5 games: 3+3+3 (3 wins each + 2 draws) = 15
        var value = ((homeTeamWins + awayTeamWins) * 3d + draws) / 15d;
        return value > 1d ? 1d : value;
    }

    public bool IsLateStage(int currentRound, int? totalRounds, int totalStandings)
    {
        if (!totalRounds.HasValue || totalRounds == 0)
            return false;

        var stagePercentage = (double)currentRound / (double)totalRounds;
        return stagePercentage > 0.8d && totalRounds > totalStandings;
    }
}
