using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using important_game.infrastructure.FootballData.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace important_game.infrastructure.FootballData;

[ExcludeFromCodeCoverage]
public sealed class FootballDataIntegration : IFootballDataIntegration
{
    private static readonly SemaphoreSlim RateLimitSemaphore = new(1, 1);
    private static readonly Queue<DateTime> RequestLog = new();
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);

    private readonly HttpClient _client;
    private readonly ILogger<FootballDataIntegration> _logger;
    private readonly JsonSerializerOptions _serializerOptions;

    public FootballDataIntegration(HttpClient client, IOptions<FootballDataOptions> options, ILogger<FootballDataIntegration> logger)
    {
        _client = client;
        _logger = logger;
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var opts = options.Value;
        if (!string.IsNullOrWhiteSpace(opts.ApiKey) && !_client.DefaultRequestHeaders.Contains("X-Auth-Token"))
        {
            _client.DefaultRequestHeaders.Add("X-Auth-Token", opts.ApiKey);
        }
    }
    public Task<FootballDataCompetition?> GetCompetitionAsync(string competitionId, CancellationToken cancellationToken = default)
        => GetAsync<FootballDataCompetition>($"competitions/{competitionId}", cancellationToken);

    public Task<FootballDataStandingsResponse?> GetCompetitionStandingsAsync(int competitionId, CancellationToken cancellationToken = default)
        => GetAsync<FootballDataStandingsResponse>($"competitions/{competitionId}/standings", cancellationToken);

    public async Task<IReadOnlyList<FootballDataMatch>> GetUpcomingMatchesAsync(int competitionId, int daysAhead = 7, CancellationToken cancellationToken = default)
    {
        var dateFrom = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
        var dateTo = DateTime.UtcNow.Date.AddDays(daysAhead).ToString("yyyy-MM-dd");
        var response = await GetAsync<FootballDataMatchesResponse>($"competitions/{competitionId}/matches?status=SCHEDULED&dateFrom={dateFrom}&dateTo={dateTo}", cancellationToken).ConfigureAwait(false);
        var matches = response?.Matches?.ToArray();
        return matches ?? Array.Empty<FootballDataMatch>();
    }

    public async Task<IReadOnlyList<FootballDataMatch>> GetTeamMatchesAsync(int teamId, string status = "FINISHED", int limit = FootballDataConstants.DefaultRecentMatchesLimit, CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<FootballDataMatchesResponse>($"teams/{teamId}/matches?status={status}&limit={limit}", cancellationToken).ConfigureAwait(false);
        var matches = response?.Matches?.ToArray();
        return matches ?? Array.Empty<FootballDataMatch>();
    }

    public Task<FootballDataHeadToHeadResponse?> GetHeadToHeadAsync(int matchId, int limit = FootballDataConstants.DefaultRecentMatchesLimit, CancellationToken cancellationToken = default)
        => GetAsync<FootballDataHeadToHeadResponse>($"matches/{matchId}/head2head?limit={limit}", cancellationToken);

    public Task<FootballDataMatch?> GetMatchAsync(int matchId, CancellationToken cancellationToken = default)
        => GetAsync<FootballDataMatch>($"matches/{matchId}", cancellationToken);

    private async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
            using (response)
            {
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Football-Data request to {Path} failed with status code {StatusCode}", path, response.StatusCode);
                    return default;
                }

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                return await JsonSerializer.DeserializeAsync<T>(stream, _serializerOptions, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Football-Data request to {Path} failed", path);
            return default;
        }
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await EnforceRateLimitAsync(cancellationToken).ConfigureAwait(false);
        return await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnforceRateLimitAsync(CancellationToken cancellationToken)
    {
        var delay = TimeSpan.Zero;

        while (true)
        {
            await RateLimitSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var now = DateTime.UtcNow;

                while (RequestLog.Count > 0 && now - RequestLog.Peek() >= RateLimitWindow)
                {
                    RequestLog.Dequeue();
                }

                if (RequestLog.Count < FootballDataConstants.FreePlanRequestsPerMinute)
                {
                    RequestLog.Enqueue(now);
                    return;
                }

                delay = RateLimitWindow - (now - RequestLog.Peek());
                if (delay < TimeSpan.Zero)
                {
                    delay = TimeSpan.Zero;
                }
            }
            finally
            {
                RateLimitSemaphore.Release();
            }

            if (delay <= TimeSpan.Zero)
            {
                continue;
            }

            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }
}
