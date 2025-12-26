using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository interface for Head-to-Head match history.
    /// Focuses on historical match data between two teams.
    /// </summary>
    public interface IHeadToHeadRepository
    {
        /// <summary>
        /// Saves head-to-head matches (insert or update).
        /// </summary>
        Task SaveHeadToHeadMatchesAsync(List<Headtohead> headtoheadMatches);
    }
}
