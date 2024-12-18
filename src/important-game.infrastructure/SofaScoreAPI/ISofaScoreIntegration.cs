﻿using important_game.infrastructure.SofaScoreAPI.Models.SofaScoreDto;

namespace important_game.infrastructure.SofaScoreAPI
{
    public interface ISofaScoreIntegration
    {
        Task<SSTournament> GetTournamentAsync(int tournamentId);
        Task<SSTournamentSeasons> GetTournamentSeasonsAsync(int tournamentId);
        Task<SSTournamentStandings> GetTournamentSeasonsTableAsync(int tournamentId, int seasonId);
        Task<SSTournamentSeasonRound> GetTournamentSeasonRoundsAsync(int tournamentId, int seasonId);
        Task<SSTournamentEvents> GetTournamentUpcomingSeasonEventsAsync(int tournamentId, int seasonId);
        Task<SSTournamentEvents> GetTeamPreviousEventsAsync(int teamId);
        Task<SSHeadToHead> GetEventH2HAsync(string eventId);
        Task<SSEventStatistics> GetEventStatisticsAsync(string eventId);
        Task<SSEventInfo> GetEventInformationAsync(string eventId);
    }
}
