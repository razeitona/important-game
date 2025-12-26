using important_game.infrastructure.Contexts.Competitions.Data.Entities;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository interface for Competition entity operations.
    /// Follows Single Responsibility Principle by focusing only on Competition data access.
    /// </summary>
    public interface ICompetitionRepository
    {
        /// <summary>
        /// Saves a competition (insert or update).
        /// </summary>
        Task SaveCompetitionAsync(CompetitionEntity competition);

        /// <summary>
        /// Gets a competition by its ID.
        /// </summary>
        Task<CompetitionEntity?> GetCompetitionByIdAsync(int id);

        /// <summary>
        /// Gets all competitions.
        /// </summary>
        Task<List<CompetitionEntity>> GetCompetitionsAsync();

        /// <summary>
        /// Gets all active competitions.
        /// </summary>
        Task<List<CompetitionEntity>> GetActiveCompetitionsAsync();
    }
}
