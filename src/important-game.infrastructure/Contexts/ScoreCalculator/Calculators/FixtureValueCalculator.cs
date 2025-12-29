using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.ScoreCalculator.Calculators;

/// <summary>
/// Interface for calculating fixture/stage value of a match.
/// Represents the progress in the season (early stage vs late stage).
/// </summary>
public interface IFixtureValueCalculator
{
    /// <summary>
    /// Calculates fixture value based on current round and total rounds.
    /// Returns a value between 0 and 1, where 1 represents the final round.
    /// </summary>
    double CalculateFixtureValue(int? currentRound, int? totalRounds);
}

/// <summary>
/// Calculates fixture/stage value based on season progress.
/// </summary>
[ExcludeFromCodeCoverage]
internal class FixtureValueCalculator : IFixtureValueCalculator
{
    public double CalculateFixtureValue(int? currentRound, int? totalRounds)
    {
        if (!currentRound.HasValue || !totalRounds.HasValue || totalRounds == 0)
            return 0d;

        return (double)currentRound / (double)totalRounds;
    }
}
