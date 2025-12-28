using important_game.infrastructure.ImportantMatch.Models.Processors;

namespace important_game.infrastructure.LeagueProcessors
{
    public interface ILeagueProcessor
    {
        /// <summary>
        /// Fetch league data information from SofaScore
        /// </summary>
        /// <param name="leagueId">League Identifier</param>
        /// <returns>League data structure</returns>
        Task<League> GetLeagueDataAsync(string leagueId);

        /// <summary>
        /// Get upcoming league fixtures
        /// </summary>
        /// <param name="leagueId">League Identifier</param>
        /// <param name="seasonId">Season Identifier</param>
        /// <returns></returns>
        Task<LeagueUpcomingFixtures> GetUpcomingMatchesAsync(string leagueId, int seasonId);

        /// <summary>
        /// Get League current table
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="seasonId"></param>
        /// <returns></returns>
        Task<LeagueStanding> GetLeagueTableAsync(string leagueId, int seasonId);


        /// <summary>
        /// Get event match statistics
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<EventStatistics?> GetEventStatisticsAsync(string eventId);


        Task<EventInfo> GetEventInformationAsync(string eventId);
    }
}


