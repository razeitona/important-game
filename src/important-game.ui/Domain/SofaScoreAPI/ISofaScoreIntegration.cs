using important_game.ui.Core.SofaScoreDto;

namespace important_game.ui.Domain.SofaScoreAPI
{
    public interface ISofaScoreIntegration
    {
        Task<SSTournament> GetTournamentAsync(int tournamentId);
        Task<SSTournamentSeasons> GetTournamentSeasonsAsync(int tournamentId);

        Task<SSTournamentEvents> GetTournamentUpcomingSeasonEventsAsync(int tournamentId, int seasonId);
        Task<SSTournamentEvents> GetTeamPreviousEventsAsync(int teamId);
        Task<SSHeadToHead> GetEventH2HAsync(int eventId);
    }
}
