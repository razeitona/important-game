using important_game.ui.Core;
using important_game.ui.Core.SofaScoreDto;
using System.Text.Json;

namespace important_game.ui.Domain.SofaScoreAPI
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

        //https://www.sofascore.com/api/v1/unique-tournament/238/season/63670/events/next/0
        public async Task<SSTournamentEvents> GetTournamentUpcomingSeasonEventsAsync(int tournamentId, int seasonId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/unique-tournament/{tournamentId}/season/{seasonId}/events/next/0";
            return await Invoke<SSTournamentEvents>(url);
        }

        //https://www.sofascore.com/api/v1/team/3001/events/last/0
        public async Task<SSTournamentEvents> GetTeamPreviousEventsAsync(int teamId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/team/{teamId}/events/last/0";
            return await Invoke<SSTournamentEvents>(url);
        }

        //https://www.sofascore.com/api/v1/event/12513997/h2h
        public async Task<SSHeadToHead> GetEventH2HAsync(int eventId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/event/{eventId}/h2h";
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
