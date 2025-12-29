using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.ScoreCalculator.Calculators;
/// <summary>
/// Interface for calculating team form based on recent match results.
/// </summary>
public interface ITeamFormCalculator
{
    /// <summary>
    /// Calculates team form score based on recent wins, draws, and losses.
    /// Returns a normalized value between 0 and 1.
    /// </summary>
    double CalculateTeamFormScore(int wins, int draws, int matches);

    /// <summary>
    /// Calculates team goals scoring potential based on recent goal statistics.
    /// Returns a value based on goals per game average.
    /// </summary>
    double CalculateTeamGoalsScore(int goalsFor, int matches);
}

/// <summary>
/// Calculates team form and goals scoring potential.
/// </summary>
[ExcludeFromCodeCoverage]
internal class TeamFormCalculator : ITeamFormCalculator
{
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
}
