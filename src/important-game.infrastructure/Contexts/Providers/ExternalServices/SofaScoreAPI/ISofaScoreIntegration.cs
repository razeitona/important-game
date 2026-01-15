using important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Models.SofaScoreDto;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI
{
    public interface ISofaScoreIntegration
    {
        /// <summary>
        /// Retrieves all currently live football matches.
        /// This is the most efficient endpoint for discovering live matches.
        /// </summary>
        Task<SSLiveEventsResponse> GetAllLiveMatchesAsync();
        Task<SSTournament> GetTournamentAsync(string tournamentId);
        Task<SSTournamentSeasons> GetTournamentSeasonsAsync(string tournamentId);
        Task<SSTournamentStandings> GetTournamentSeasonsTableAsync(string tournamentId, int seasonId);
        Task<SSTournamentSeasonRound> GetTournamentSeasonRoundsAsync(string tournamentId, int seasonId);
        Task<SSTournamentEvents> GetTournamentUpcomingSeasonEventsAsync(string tournamentId, int seasonId);
        Task<SSTournamentEvents> GetTeamPreviousEventsAsync(int teamId);
        Task<SSHeadToHead> GetEventH2HAsync(string eventId);
        Task<SSEventStatistics> GetEventStatisticsAsync(string eventId);
        Task<SSEventInfo> GetEventInformationAsync(string eventId);
    }
}
