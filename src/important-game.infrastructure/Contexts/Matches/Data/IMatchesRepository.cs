using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Contexts.Matches.Data;

/// <summary>
/// Repository interface for Matches entity operations.
/// Follows Single Responsibility Principle by focusing only on Matches data access.
/// </summary>
public interface IMatchesRepository
{
    /// <summary>
    /// Gets unfinished matches (IsFinished = 0).
    /// </summary>
    Task<List<UnfinishedMatchDto>> GetUnfinishedMatchesAsync();

    /// <summary>
    /// Saves a match (insert or update) with all its related data.
    /// </summary>
    Task<MatchesEntity> SaveMatchAsync(MatchesEntity entity);

    /// <summary>
    /// Update Match entity with new calculated excitement score
    /// </summary>
    Task UpdateMatchCalculatorAsync(MatchCalcsDto entity);

    /// <summary>
    /// Gets all unfinished matches (IsFinished = 0) with team information.
    /// </summary>
    Task<List<MatchDto>> GetAllUnfinishedMatchesAsync();
    Task<MatchDetailDto?> GetMatchByIdAsync(int matchId);
    Task<MatchDetailDto?> GetMatchByTeamSlugsAsync(string homeSlug, string awaySlug);
    Task<DateTimeOffset?> GetTeamLastFinishedMatchDateAsync(int teamId);
    Task<List<MatchesEntity>> GetRecentMatchesForTeamAsync(int teamId, int numberOfMatches);
    Task<bool> HasRecentFinishedMatchAsync(int competitionId, int seasonId, DateTimeOffset? dateTime);
    Task<List<MatchDto>> GetUserFavoriteUpcomingMatchesAsync(int userId);

    #region Head To Head
    Task<List<HeadToHeadDto>> GetHeadToHeadMatchesAsync(int teamOneId, int teamTwoId);
    #endregion

    #region Rivalry
    Task<RivalryEntity?> GetRivalryAsync(int teamOneId, int teamTwoId);
    #endregion
}
