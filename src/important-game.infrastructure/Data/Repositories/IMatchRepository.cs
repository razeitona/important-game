using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository interface for Match entity operations.
    /// Follows Single Responsibility Principle by focusing only on Match data access.
    /// </summary>
    [Obsolete("Class to be removed")]
    public interface IMatchRepository
    {
        /// <summary>
        /// Saves a match (insert or update) with all its related data.
        /// </summary>
        [Obsolete("Class to be removed")]
        Task SaveMatchAsync(Match match);

        /// <summary>
        /// Gets a match by ID with all related entities (teams, competition, live data, head-to-head).
        /// </summary>
        [Obsolete("Class to be removed")]
        Task<Match?> GetMatchByIdAsync(int id);

        /// <summary>
        /// Gets all upcoming matches (not finished).
        /// </summary>
        [Obsolete("Class to be removed")]
        Task<List<Match>> GetUpcomingMatchesAsync();

        /// <summary>
        /// Gets all active (not finished) matches from a specific competition.
        /// </summary>
        [Obsolete("Class to be removed")]
        Task<List<Match>> GetCompetitionActiveMatchesAsync(int competitionId);

        /// <summary>
        /// Gets all unfinished matches.
        /// </summary>
        [Obsolete("Class to be removed")]
        Task<List<Match>> GetUnfinishedMatchesAsync();
    }
}
