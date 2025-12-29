using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Contexts.Matches.Data;

/// <summary>
/// Repository interface for Matches entity operations.
/// Follows Single Responsibility Principle by focusing only on Matches data access.
/// </summary>
public interface IMatchesRepository
{
    /// <summary>
    /// Gets all unfinished matches (IsFinished = 0).
    /// </summary>
    Task<List<MatchesEntity>> GetUnfinishedMatchesAsync();

    /// <summary>
    /// Saves a match (insert or update) with all its related data.
    /// </summary>
    Task<MatchesEntity> SaveFinishedMatchAsync(MatchesEntity entity);

    /// <summary>
    /// Update Match entity with new calculated excitement score
    /// </summary>
    Task UpdateMatchCalculatorAsync(MatchCalcsDto entity);

    Task<RivalryEntity?> GetRivalryAsync(int teamOneId, int teamTwoId);

    Task<List<MatchDto>> GetAllUpcomingMatchesAsync();
}
