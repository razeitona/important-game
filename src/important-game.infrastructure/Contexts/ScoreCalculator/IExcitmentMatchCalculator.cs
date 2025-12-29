namespace important_game.infrastructure.Contexts.ScoreCalculator;

/// <summary>
/// Interface for calculating excitement scores of unfinished matches.
/// Follows Single Responsibility Principle by focusing only on excitement score calculations.
/// </summary>
public interface IExcitmentMatchCalculator
{
    /// <summary>
    /// Calculates and updates excitement scores for all unfinished matches (IsFinished = 0).
    /// Processes matches in parallel with thread-safe semaphore control.
    /// </summary>
    Task CalculateExcitmentScoreAsync(CancellationToken cancellationToken = default);
}
