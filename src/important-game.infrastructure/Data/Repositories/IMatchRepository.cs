using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository interface for Match entity operations.
    /// Follows Single Responsibility Principle by focusing only on Match data access.
    /// </summary>
    public interface IMatchRepository
    {
        /// <summary>
        /// Saves a match (insert or update) with all its related data.
        /// </summary>
        Task SaveMatchAsync(Match match);

        /// <summary>
        /// Saves multiple matches in bulk.
        /// </summary>
        Task SaveMatchesAsync(List<Match> matches);

        /// <summary>
        /// Gets a match by ID with all related entities (teams, competition, live data, head-to-head).
        /// </summary>
        Task<Match?> GetMatchByIdAsync(int id);

        /// <summary>
        /// Gets all upcoming matches (not finished).
        /// </summary>
        Task<List<Match>> GetUpcomingMatchesAsync();

        /// <summary>
        /// Gets all matches from a specific competition.
        /// </summary>
        Task<List<Match>> GetMatchesFromCompetitionAsync(int competitionId);

        /// <summary>
        /// Gets all active (not finished) matches from a specific competition.
        /// </summary>
        Task<List<Match>> GetCompetitionActiveMatchesAsync(int competitionId);

        /// <summary>
        /// Gets upcoming matches from a specific competition.
        /// </summary>
        Task<List<Match>> GetUpcomingMatchesFromCompetitionAsync(int competitionId);

        /// <summary>
        /// Gets live matches from a specific competition.
        /// </summary>
        Task<List<Match>> GetLiveMatchesFromCompetitionAsync(int competitionId);

        /// <summary>
        /// Gets all unfinished matches.
        /// </summary>
        Task<List<Match>> GetUnfinishedMatchesAsync();

        /// <summary>
        /// Gets all finished matches from a specific competition.
        /// </summary>
        Task<List<Match>> GetFinishedMatchesFromCompetitionAsync(int competitionId);
    }
}
