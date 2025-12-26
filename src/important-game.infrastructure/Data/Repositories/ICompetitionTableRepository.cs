using important_game.infrastructure.Contexts.Competitions.Data.Entities;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository interface for CompetitionTable entities.
    /// Handles persistence of competition standing snapshots.
    /// </summary>
    public interface ICompetitionTableRepository
    {
        /// <summary>
        /// Save or update a competition table entry.
        /// </summary>
        Task SaveAsync(CompetitionTableEntity table);

        /// <summary>
        /// Save or update multiple competition table entries for a competition.
        /// Replaces all existing entries for that competition.
        /// </summary>
        Task SaveAllAsync(List<CompetitionTableEntity> tables);

        /// <summary>
        /// Get all standings for a specific competition, ordered by position.
        /// </summary>
        Task<List<CompetitionTableEntity>> GetByCompetitionIdAsync(int competitionId);

        /// <summary>
        /// Get standing for a specific team in a competition.
        /// </summary>
        Task<CompetitionTableEntity?> GetByTeamAndCompetitionAsync(int teamId, int competitionId);

        /// <summary>
        /// Get the last update timestamp for a competition's standings.
        /// </summary>
        Task<DateTime?> GetLastUpdateAsync(int competitionId);

        /// <summary>
        /// Delete all standings for a competition.
        /// Used when refreshing data from Gemini API.
        /// </summary>
        Task DeleteByCompetitionIdAsync(int competitionId);
    }
}
