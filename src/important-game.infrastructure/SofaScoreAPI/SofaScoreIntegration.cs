using important_game.infrastructure.SofaScoreAPI.Models;
using important_game.infrastructure.SofaScoreAPI.Models.SofaScoreDto;
using System.Text.Json;

namespace important_game.infrastructure.SofaScoreAPI
{
    internal class SofaScoreIntegration(HttpClient httpClient) : ISofaScoreIntegration
    {

        public async Task<SSTournament> GetTournamentAsync(int tournamentId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/unique-tournament/{tournamentId}";
            return await Invoke<SSTournament>(url);
        }

        public async Task<SSTournamentSeasons> GetTournamentSeasonsAsync(int tournamentId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/unique-tournament/{tournamentId}/seasons";
            return await Invoke<SSTournamentSeasons>(url);
        }

        public async Task<SSTournamentStandings> GetTournamentSeasonsTableAsync(int tournamentId, int seasonId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/unique-tournament/{tournamentId}/season/{seasonId}/standings/total";
            return await Invoke<SSTournamentStandings>(url);
        }

        public async Task<SSTournamentEvents> GetTournamentUpcomingSeasonEventsAsync(int tournamentId, int seasonId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/unique-tournament/{tournamentId}/season/{seasonId}/events/next/0";
            return await Invoke<SSTournamentEvents>(url);
        }

        public async Task<SSTournamentSeasonRound> GetTournamentSeasonRoundsAsync(int tournamentId, int seasonId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/unique-tournament/{tournamentId}/season/{seasonId}/rounds";
            return await Invoke<SSTournamentSeasonRound>(url);
        }

        public async Task<SSTournamentEvents> GetTeamPreviousEventsAsync(int teamId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/team/{teamId}/events/last/0";
            return await Invoke<SSTournamentEvents>(url);
        }

        public async Task<SSHeadToHead> GetEventH2HAsync(string customEventId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/event/{customEventId}/h2h/events";
            return await Invoke<SSHeadToHead>(url);
        }




        private async Task<T> Invoke<T>(string url) where T : class
        {
            try
            {
                var response = await httpClient.GetAsync(url);

                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return JsonSerializer.Deserialize<T>(responseContent);

            }
            catch (Exception ex)
            {
                return default;
            }
        }
    }
}
