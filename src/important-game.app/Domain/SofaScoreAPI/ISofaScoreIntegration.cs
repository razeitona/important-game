using important_game.ui.Core.SofaScoreDto;

namespace important_game.ui.Domain.SofaScoreAPI
{
    public interface ISofaScoreIntegration
    {
        Task<SSSportTournaments> GetSportTournamentAsync(string sportId, DateTime date);
        Task<SSTournament> GetTournamentAsync(int tournamentId);
        Task<SSTournamentSeasons> GetTournamentSeasonsAsync(int tournamentId);
        Task<SSTournamentStandings> GetTournamentSeasonsTableAsync(int tournamentId, int seasonId);
        Task<SSTournamentSeasonRound> GetTournamentSeasonRoundsAsync(int tournamentId, int seasonId);

        Task<SSTournamentEvents> GetTournamentUpcomingSeasonEventsAsync(int tournamentId, int seasonId);
        Task<SSTournamentEvents> GetTournamentOngoingSeasonEventsAsync(int tournamentId, int seasonId);
        Task<SSTournamentEvents> GetTeamPreviousEventsAsync(int teamId);
        Task<SSHeadToHead> GetEventH2HAsync(string customEventId);
    }
}
