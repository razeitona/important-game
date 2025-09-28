using important_game.infrastructure.SofaScoreAPI.Models;
using important_game.infrastructure.SofaScoreAPI.Models.SofaScoreDto;
using PuppeteerSharp;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace important_game.infrastructure.SofaScoreAPI
{
    [ExcludeFromCodeCoverage]
    internal class SofaScoreIntegration : ISofaScoreIntegration
    {
        private static readonly MemoryCache ResponseCache = new(new MemoryCacheOptions
        {
            SizeLimit = 128
        });

        private static readonly TimeSpan CacheEntrySlidingExpiration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan CacheEntryAbsoluteExpiration = TimeSpan.FromMinutes(20);

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
            if (ResponseCache.TryGetValue(url, out var cached) && cached is T cachedValue)
            {
                return cachedValue;
            }

            try
            {
                var browserFetcher = new BrowserFetcher();

                await browserFetcher.DownloadAsync();
                await using var browser = await Puppeteer.LaunchAsync(
                    new LaunchOptions { Headless = true });

                await using var page = await browser.NewPageAsync();
                await page.GoToAsync("https://www.sofascore.com");

                var response = await page.GoToAsync(url);
                if (response == null)
                {
                    return default;
                }

                var responseContent = await response.TextAsync();
                var result = JsonSerializer.Deserialize<T>(responseContent);

                if (result != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSize(1)
                        .SetSlidingExpiration(CacheEntrySlidingExpiration)
                        .SetAbsoluteExpiration(CacheEntryAbsoluteExpiration);

                    ResponseCache.Set(url, result, cacheEntryOptions);
                }

                return result;

            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception)
            {
                return default;
            }
        }


        private void ProcessResponse(object? sender, ResponseCreatedEventArgs e)
        {
        }
    }


}

