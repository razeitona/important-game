using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Models;
using important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Models.SofaScoreDto;
using important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Utils;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI
{
    /// <summary>
    /// Optimized SofaScore integration using HttpClient instead of Puppeteer.
    /// Implements rate limiting (30s between requests) and retry logic to avoid Cloudflare blocking.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SofaScoreIntegration : ISofaScoreIntegration
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SofaScoreIntegration> _logger;
        private readonly SofaScoreRateLimiter _rateLimiter;
        private readonly IMemoryCache _cache;

        private static readonly TimeSpan CacheEntrySlidingExpiration = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan CacheEntryAbsoluteExpiration = TimeSpan.FromMinutes(10);
        private const int MaxRetries = 3;

        public SofaScoreIntegration(
            HttpClient httpClient,
            ILogger<SofaScoreIntegration> logger,
            SofaScoreRateLimiter rateLimiter,
            IMemoryCache cache)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Retrieves all currently live football matches in a single request.
        /// This is the most efficient way to discover live matches.
        /// </summary>
        public async Task<SSLiveEventsResponse> GetAllLiveMatchesAsync()
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/sport/football/events/live";
            return await InvokeAsync<SSLiveEventsResponse>(url, "GetAllLiveMatches");
        }

        public async Task<SSTournament> GetTournamentAsync(string tournamentId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/unique-tournament/{tournamentId}";
            return await InvokeAsync<SSTournament>(url, $"GetTournament({tournamentId})");
        }

        public async Task<SSTournamentSeasons> GetTournamentSeasonsAsync(string tournamentId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/unique-tournament/{tournamentId}/seasons";
            return await InvokeAsync<SSTournamentSeasons>(url, $"GetTournamentSeasons({tournamentId})");
        }

        public async Task<SSTournamentStandings> GetTournamentSeasonsTableAsync(string tournamentId, int seasonId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/unique-tournament/{tournamentId}/season/{seasonId}/standings/total";
            return await InvokeAsync<SSTournamentStandings>(url, $"GetTournamentTable({tournamentId}/{seasonId})");
        }

        public async Task<SSTournamentEvents> GetTournamentUpcomingSeasonEventsAsync(string tournamentId, int seasonId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/unique-tournament/{tournamentId}/season/{seasonId}/events/next/0";
            return await InvokeAsync<SSTournamentEvents>(url, $"GetUpcomingEvents({tournamentId}/{seasonId})");
        }

        public async Task<SSTournamentSeasonRound> GetTournamentSeasonRoundsAsync(string tournamentId, int seasonId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/unique-tournament/{tournamentId}/season/{seasonId}/rounds";
            return await InvokeAsync<SSTournamentSeasonRound>(url, $"GetSeasonRounds({tournamentId}/{seasonId})");
        }

        public async Task<SSTournamentEvents> GetTeamPreviousEventsAsync(int teamId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/team/{teamId}/events/last/0";
            return await InvokeAsync<SSTournamentEvents>(url, $"GetTeamPreviousEvents({teamId})");
        }

        public async Task<SSHeadToHead> GetEventH2HAsync(string eventId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/event/{eventId}/h2h/events";
            return await InvokeAsync<SSHeadToHead>(url, $"GetEventH2H({eventId})");
        }

        public async Task<SSEventStatistics> GetEventStatisticsAsync(string eventId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/event/{eventId}/statistics";
            return await InvokeAsync<SSEventStatistics>(url, $"GetEventStatistics({eventId})");
        }

        public async Task<SSEventInfo> GetEventInformationAsync(string eventId)
        {
            var url = $"{SofaScoreConstants.BaseUrl}api/v1/event/{eventId}";
            return await InvokeAsync<SSEventInfo>(url, $"GetEventInfo({eventId})");
        }

        /// <summary>
        /// Core method that handles HTTP requests with caching, rate limiting, and retry logic.
        /// </summary>
        private async Task<T> InvokeAsync<T>(string url, string operationName) where T : class
        {
            // Check cache first
            if (_cache.TryGetValue(url, out var cached) && cached is T cachedValue)
            {
                _logger.LogDebug("{Operation} - Cache hit for {Url}", operationName, url);
                return cachedValue;
            }

            // Execute with rate limiting
            return await _rateLimiter.ExecuteWithRateLimitAsync(async () =>
            {
                return await ExecuteWithRetryAsync(async () =>
                {
                    try
                    {
                        _logger.LogInformation("{Operation} - Fetching from {Url}", operationName, url);

                        var response = await _httpClient.GetAsync(url);

                        // Handle specific status codes
                        if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            _logger.LogWarning(
                                "{Operation} - Received 403 Forbidden from SofaScore. Cloudflare may be blocking requests.",
                                operationName);
                            throw new HttpRequestException(
                                "SofaScore returned 403 Forbidden - possible Cloudflare block",
                                null,
                                HttpStatusCode.Forbidden);
                        }

                        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            _logger.LogWarning(
                                "{Operation} - Received 503 Service Unavailable from SofaScore. Varnish server may be overloaded.",
                                operationName);
                            throw new HttpRequestException(
                                "SofaScore returned 503 Service Unavailable - server overload",
                                null,
                                HttpStatusCode.ServiceUnavailable);
                        }

                        if (response.StatusCode == (HttpStatusCode)429)
                        {
                            _logger.LogWarning(
                                "{Operation} - Received 429 Too Many Requests from SofaScore.",
                                operationName);
                            throw new HttpRequestException(
                                "SofaScore returned 429 Too Many Requests - rate limit exceeded",
                                null,
                                (HttpStatusCode)429);
                        }

                        response.EnsureSuccessStatusCode();

                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrWhiteSpace(responseContent))
                        {
                            _logger.LogWarning("{Operation} - Empty response from SofaScore", operationName);
                            return default;
                        }

                        var result = JsonSerializer.Deserialize<T>(responseContent);

                        if (result != null)
                        {
                            // Cache successful results
                            var cacheEntryOptions = new MemoryCacheEntryOptions()
                                .SetSize(1)
                                .SetSlidingExpiration(CacheEntrySlidingExpiration)
                                .SetAbsoluteExpiration(CacheEntryAbsoluteExpiration);

                            _cache.Set(url, result, cacheEntryOptions);
                            _logger.LogDebug("{Operation} - Successfully cached response", operationName);
                        }

                        return result;
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogError(ex, "{Operation} - HTTP request failed", operationName);
                        throw;
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "{Operation} - JSON deserialization failed", operationName);
                        return default;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{Operation} - Unexpected error", operationName);
                        return default;
                    }
                }, operationName);
            }, operationName);
        }

        /// <summary>
        /// Executes an operation with exponential backoff retry logic.
        /// Handles 403 (Cloudflare), 503 (Server overload), and 429 (Rate limit) errors.
        /// </summary>
        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (HttpRequestException ex) when (attempt < MaxRetries && ShouldRetry(ex.StatusCode))
                {
                    var delay = CalculateBackoffDelay(attempt);

                    _logger.LogWarning(
                        "{Operation} - Request failed (attempt {Attempt}/{MaxRetries}). " +
                        "Status: {Status}. Retrying in {Delay}s...",
                        operationName,
                        attempt,
                        MaxRetries,
                        ex.StatusCode,
                        delay.TotalSeconds);

                    await Task.Delay(delay);
                }
            }

            _logger.LogError(
                "{Operation} - Failed after {MaxRetries} attempts",
                operationName,
                MaxRetries);

            throw new Exception($"{operationName} failed after {MaxRetries} attempts");
        }

        /// <summary>
        /// Determines if a request should be retried based on HTTP status code.
        /// </summary>
        private static bool ShouldRetry(HttpStatusCode? statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.Forbidden => true,            // 403 - Cloudflare block
                HttpStatusCode.ServiceUnavailable => true,   // 503 - Varnish server overload
                (HttpStatusCode)429 => true,                 // 429 - Too many requests
                HttpStatusCode.RequestTimeout => true,       // 408 - Request timeout
                HttpStatusCode.GatewayTimeout => true,       // 504 - Gateway timeout
                _ => false
            };
        }

        /// <summary>
        /// Calculates exponential backoff delay for retry attempts.
        /// Attempt 1: 30s, Attempt 2: 60s, Attempt 3: 120s
        /// </summary>
        private static TimeSpan CalculateBackoffDelay(int attempt)
        {
            return TimeSpan.FromSeconds(Math.Pow(2, attempt) * 30);
        }
    }
}
