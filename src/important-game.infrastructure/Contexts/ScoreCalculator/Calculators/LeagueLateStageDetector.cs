using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.ScoreCalculator.Calculators;

/// <summary>
/// Interface for determining if match is in late stage of season.
/// </summary>
public interface ILeagueLateStageDetector
{
    /// <summary>
    /// Determines if the current round represents late stage of the season.
    /// Late stage is typically after 80% of the season.
    /// </summary>
    bool IsLateStage(int currentRound, int? totalRounds, int totalStandings);
}

/// <summary>
/// Determines if match is in late stage of season.
/// </summary>
[ExcludeFromCodeCoverage]
internal class LeagueLateStageDetector : ILeagueLateStageDetector
{
    public bool IsLateStage(int currentRound, int? totalRounds, int totalStandings)
    {
        if (!totalRounds.HasValue || totalRounds == 0)
            return false;

        var stagePercentage = (double)currentRound / (double)totalRounds;
        return stagePercentage > 0.8d && totalRounds > totalStandings;
    }
}
