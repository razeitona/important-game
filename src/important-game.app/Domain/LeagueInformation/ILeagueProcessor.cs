using important_game.ui.Core;
using important_game.ui.Core.Models;

namespace important_game.ui.Domain.LeagueInformation
{
    public interface ILeagueProcessor
    {
        /// <summary>
        /// Fetch league data information from SofaScore
        /// </summary>
        /// <param name="leagueId">League Identifier</param>
        /// <returns>League data structure</returns>
        Task<League> GetLeagueDataAsync(MatchImportanceLeague league);

        /// <summary>
        /// Get upcoming league fixtures
        /// </summary>
        /// <param name="leagueId">League Identifier</param>
        /// <param name="seasonId">Season Identifier</param>
        /// <returns></returns>
        Task<LeagueUpcomingFixtures> GetUpcomingFixturesAsync(int leagueId, int seasonId);

        /// <summary>
        /// Get League current table
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="seasonId"></param>
        /// <returns></returns>
        Task<LeagueStanding> GetLeagueTableAsync(int leagueId, int seasonId);
    }
}
