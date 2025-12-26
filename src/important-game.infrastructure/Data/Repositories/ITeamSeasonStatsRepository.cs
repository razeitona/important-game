using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository interface for TeamSeasonStats entities.
    /// Handles persistence of team statistics snapshots from Gemini API.
    /// </summary>
    public interface ITeamSeasonStatsRepository
    {
        /// <summary>
        /// Save or update team season statistics.
        /// </summary>
        Task SaveAsync(TeamSeasonStats stats);

        /// <summary>
        /// Save or update multiple team season statistics.
        /// </summary>
        Task SaveAllAsync(List<TeamSeasonStats> statsList);

        /// <summary>
        /// Get statistics for a specific team in a competition.
        /// </summary>
        Task<TeamSeasonStats?> GetByTeamAndCompetitionAsync(int teamId, int competitionId);

        /// <summary>
        /// Get all statistics for a competition.
        /// </summary>
        Task<List<TeamSeasonStats>> GetByCompetitionAsync(int competitionId);

        /// <summary>
        /// Get statistics older than a specific timestamp.
        /// Used to identify data that needs refresh from API.
        /// </summary>
        Task<List<TeamSeasonStats>> GetOlderThanAsync(DateTime threshold);

        /// <summary>
        /// Delete statistics for a specific team in a competition.
        /// </summary>
        Task DeleteByTeamAndCompetitionAsync(int teamId, int competitionId);

        /// <summary>
        /// Delete all statistics for a competition.
        /// </summary>
        Task DeleteByCompetitionAsync(int competitionId);
    }
}
