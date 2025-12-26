using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository interface for Team entity operations.
    /// Follows Single Responsibility Principle by focusing only on Team data access.
    /// </summary>
    public interface ITeamRepository
    {
        /// <summary>
        /// Saves a team (insert or update).
        /// </summary>
        Task<Team> SaveTeamAsync(Team team);

        /// <summary>
        /// Gets a team by its ID.
        /// </summary>
        Task<Team?> GetTeamByIdAsync(int id);

        /// <summary>
        /// Gets multiple teams by their IDs.
        /// </summary>
        Task<List<Team>> GetTeamsByIdsAsync(List<int> ids);
    }
}
