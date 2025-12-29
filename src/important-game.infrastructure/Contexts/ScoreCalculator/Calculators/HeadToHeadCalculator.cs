using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.ScoreCalculator.Calculators;
/// <summary>
/// Interface for calculating head-to-head match history value.
/// </summary>
public interface IHeadToHeadCalculator
{
    /// <summary>
    /// Calculates head-to-head score based on historical match results between two teams.
    /// Returns a normalized value between 0 and 1.
    /// </summary>
    double CalculateHeadToHeadScore(int homeTeamWins, int awayTeamWins, int draws);
}



/// <summary>
/// Calculates head-to-head match history value.
/// </summary>
[ExcludeFromCodeCoverage]
internal class HeadToHeadCalculator : IHeadToHeadCalculator
{
    public double CalculateHeadToHeadScore(int homeTeamWins, int awayTeamWins, int draws)
    {
        if (homeTeamWins == 0 && awayTeamWins == 0 && draws == 0)
            return 0.5d;

        // Maximum value from 5 games: 3+3+3 (3 wins each + 2 draws) = 15
        var value = ((homeTeamWins + awayTeamWins) * 3d + draws) / 15d;
        return value > 1d ? 1d : value;
    }
}
