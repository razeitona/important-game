using important_game.infrastructure.Contexts.Competitions.Data.Entities;

namespace important_game.infrastructure.Contexts.Competitions.Data;

/// <summary>
/// Repository interface for Competitions entities operations.
/// Follows Single Responsibility Principle by focusing only on Competition data access.
/// </summary>
public interface ICompetitionRepository
{

    #region Competitions
    /// <summary>
    /// Saves a competition (insert or update).
    /// </summary>
    Task SaveCompetitionAsync(CompetitionEntity competition);

    /// <summary>
    /// Gets a competition by its ID.
    /// </summary>
    Task<CompetitionEntity?> GetCompetitionByIdAsync(int competitionId);

    /// <summary>
    /// Gets all competitions.
    /// </summary>
    Task<List<CompetitionEntity>> GetCompetitionsAsync();

    /// <summary>
    /// Gets all active competitions.
    /// </summary>
    Task<List<CompetitionEntity>> GetActiveCompetitionsAsync();
    #endregion

    #region Competition Seasons
    Task<CompetitionSeasonsEntity> SaveCompetitionSeasonAsync(CompetitionSeasonsEntity entity);
    Task<CompetitionSeasonsEntity?> GetCompetitionSeasonByIdAsync(int seasonId);
    Task<CompetitionSeasonsEntity?> GetCompetitionSeasonByCompetitionAndYearAsync(int competitionId, string seasonYear);
    Task<List<CompetitionSeasonsEntity>> GetCompetitionSeasonsByCompetitionAsync(int competitionId);
    Task<CompetitionSeasonsEntity?> GetLatestCompetitionSeasonAsync(int competitionId);
    Task DeleteCompetitionSeasonAsync(int seasonId);
    Task UpdateCompetitionSeasonStandingDateAsync(int seasonId, DateTimeOffset updateDate);

    #endregion

    #region Competition Table
    /// <summary>
    /// Save or update a competition table entry.
    /// </summary>
    Task SaveCompetitionTableAsync(List<CompetitionTableEntity> table);

    /// <summary>
    /// Delete all standings for a competition and season.
    /// Used when refreshing data from external integration.
    /// </summary>
    Task DeleteCompetitionTableByCompetitionAndSeasonAsync(int competitionId, int seasonId);

    /// <summary>
    /// Get all standings for a specific competition and season, ordered by position.
    /// </summary>
    Task<List<CompetitionTableEntity>> GetCompetitionTableAsync(int competitionId, int seasonId);
    #endregion

}
