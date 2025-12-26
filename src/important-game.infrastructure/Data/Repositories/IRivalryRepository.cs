using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository interface for Rivalry entity operations.
    /// Follows Single Responsibility Principle by focusing only on Rivalry data access.
    /// </summary>
    public interface IRivalryRepository
    {
        /// <summary>
        /// Saves a rivalry (insert or update).
        /// </summary>
        Task SaveRivalryAsync(Rivalry rivalry);

        /// <summary>
        /// Gets rivalry between two teams (bidirectional).
        /// </summary>
        Task<Rivalry?> GetRivalryByTeamIdAsync(int teamOneId, int teamTwoId);
    }
}
