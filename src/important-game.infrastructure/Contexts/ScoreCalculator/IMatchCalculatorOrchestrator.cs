namespace important_game.infrastructure.Contexts.ScoreCalculator;

/// <summary>
/// Interface for calculating excitement scores of unfinished matches.
/// Follows Single Responsibility Principle by focusing only on excitement score calculations.
/// </summary>
public interface IMatchCalculatorOrchestrator
{
    /// <summary>
    /// Calculates and updates excitement scores for all unfinished matches (IsFinished = 0).
    /// Processes matches in parallel with thread-safe semaphore control.
    /// </summary>
    Task CalculateExcitmentScoreAsync(bool skipDateCondition = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates excitement scores for currently live matches using SofaScore data.
    /// Updates scores based on live match events (goals, cards, shots, etc.).
    /// Maps SofaScore event IDs to internal match IDs using fuzzy matching.
    /// Processes top priority matches to stay within API rate limits.
    /// </summary>
    Task CalculateLiveScoreAsync(CancellationToken cancellationToken = default);
}
