using important_game.infrastructure.SofaScoreAPI.Models;
using important_game.infrastructure.SofaScoreAPI.Models.SofaScoreDto;
using PuppeteerSharp;
using System.Text.Json;

namespace important_game.infrastructure.SofaScoreAPI
{
    internal class SofaScoreIntegration : ISofaScoreIntegration
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

        public async Task<SSHeadToHead> GetEventH2HAsync(string eventId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/event/{eventId}/h2h/events";
            return await Invoke<SSHeadToHead>(url);
        }

        public async Task<SSEventStatistics> GetEventStatisticsAsync(string eventId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/event/{eventId}/statistics";
            return await Invoke<SSEventStatistics>(url);
        }

        public async Task<SSEventInfo> GetEventInformationAsync(string eventId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/event/{eventId}";
            return await Invoke<SSEventInfo>(url);
        }


        private async Task<T> Invoke<T>(string url) where T : class
        {
            try
            {
                var browserFetcher = new BrowserFetcher();


                await browserFetcher.DownloadAsync();
                await using var browser = await Puppeteer.LaunchAsync(
                    new LaunchOptions { Headless = true });

                await using var page = await browser.NewPageAsync();
                await page.GoToAsync("https://www.sofascore.com");

                var response = await page.GoToAsync(url);
                var responseContent = await response.TextAsync();

                return JsonSerializer.Deserialize<T>(responseContent);

            }
            catch (HttpRequestException ex)
            {
                // Log the full exception details
                throw; // Re-throw to allow caller to handle
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        private void ProcessResponse(object? sender, ResponseCreatedEventArgs e)
        {
        }
    }


}
