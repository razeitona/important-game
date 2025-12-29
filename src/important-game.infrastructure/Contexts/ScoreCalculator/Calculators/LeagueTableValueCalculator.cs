using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.ScoreCalculator.Calculators;


/// <summary>
/// Interface for calculating table position value based on standings.
/// </summary>
public interface ILeagueTableValueCalculator
{
    /// <summary>
    /// Calculates league table value based on team positions and point difference.
    /// Considers position difference, top/bottom matchup value, and point impact.
    /// Returns a normalized value between 0 and 1.
    /// </summary>
    double CalculateLeagueTableValue(
        int homeTeamPosition,
        int awayTeamPosition,
        int homeTeamPoints,
        int awayTeamPoints,
        int homeTeamMatches,
        int awayTeamMatches,
        int totalTeams,
        int totalRounds);
}



/// <summary>
/// Calculates league table position value.
/// </summary>
[ExcludeFromCodeCoverage]
internal class LeagueTableValueCalculator : ILeagueTableValueCalculator
{
    public double CalculateLeagueTableValue(
        int homeTeamPosition,
        int awayTeamPosition,
        int homeTeamPoints,
        int awayTeamPoints,
        int homeTeamMatches,
        int awayTeamMatches,
        int totalTeams,
        int totalRounds)
    {
        if (totalTeams == 0 || totalRounds == 0)
            return 0d;

        if (totalRounds <= 1)
            return 0.5d;

        var positionDiff = Math.Abs(homeTeamPosition - awayTeamPosition) - 1d;
        var totalTeamsDouble = (double)totalTeams;

        var positionValue = 1d / (1d + (positionDiff / (totalTeamsDouble - 1d)));
        var averageTeamPosition = (homeTeamPosition + awayTeamPosition) / 2d;
        var topBottomMatchupValue = 1d - (averageTeamPosition - 1d) / (totalTeamsDouble - 1d);

        var pointDifference = Math.Abs(homeTeamPoints - awayTeamPoints);
        var maxPointDifference = (totalRounds - Math.Max(homeTeamMatches, awayTeamMatches)) * 3d;

        if (maxPointDifference <= 0)
            return positionValue * topBottomMatchupValue;

        var pointImpactValue = 1d / (1d + (pointDifference / (maxPointDifference - 1d)));

        return positionValue * topBottomMatchupValue * pointImpactValue;
    }
}
