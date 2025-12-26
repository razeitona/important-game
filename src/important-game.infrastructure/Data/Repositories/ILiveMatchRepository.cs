using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository interface for LiveMatch entity operations.
    /// Follows Single Responsibility Principle by focusing only on LiveMatch data access.
    /// </summary>
    public interface ILiveMatchRepository
    {
        /// <summary>
        /// Saves a live match snapshot (insert or update).
        /// </summary>
        Task SaveLiveMatchAsync(LiveMatch liveMatch);

        /// <summary>
        /// Saves multiple live match snapshots.
        /// </summary>
        Task SaveLiveMatchesAsync(List<LiveMatch> liveMatches);

        /// <summary>
        /// Gets a live match snapshot by ID.
        /// </summary>
        Task<LiveMatch?> GetLiveMatchByIdAsync(int id);
    }
}
