using important_game.infrastructure.Contexts.Providers.Data.Entities;
using important_game.infrastructure.Contexts.Providers.ExternalServices.FootballData.Mappers;
using important_game.infrastructure.Contexts.Providers.ExternalServices.FootballData.Models;
using important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations;
using important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.FootballData;

[ExcludeFromCodeCoverage]
public sealed class FootballDataProvider : IExternalIntegrationProvider,
    IExternalCompetitionProvider, IExternalMatchProvider
{
    public int Id => 1;
    public string Name => "FootballData";

    private readonly ILogger<FootballDataProvider> _logger;
    private readonly HttpClient _client;
    private readonly IExternalProviderSettings _providerLogger;

    private static readonly SemaphoreSlim RateLimitSemaphore = new(1, 1);
    private readonly JsonSerializerOptions _serializerOptions;
    private ConcurrentBag<ExternalProvidersLogsEntity> _logs = new();
    private ExternalProvidersEntity? _settings;

    public FootballDataProvider(ILogger<FootballDataProvider> logger, HttpClient client, IOptions<FootballDataOptions> options, IExternalProviderSettings providerLogger)
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
        _providerLogger = providerLogger;
    }

    #region External Competition Methods
    public async Task<ExternalCompetitionDto?> GetCompetitionAsync(string competitionId, CancellationToken cancellationToken = default)
    {
        var footballDataCompetition = await InternalGetCompetitionAsync(competitionId, cancellationToken);
        if (footballDataCompetition == null)
            return null;

        var externalCompetition = FootballDataMapper.MapToExternalCompetition(footballDataCompetition);
        return externalCompetition;
    }

    public async Task<ExternalCompetitionStandingsDto?> GetCompetitionStandingsAsync(string competitionId, CancellationToken cancellationToken = default)
    {
        var footballDataStandings = await InternalGetCompetitionStandingsAsync(competitionId, cancellationToken);
        if (footballDataStandings == null)
            return null;

        var competitionStandings = FootballDataMapper.MapToExternalCompetitionStandings(footballDataStandings);
        return competitionStandings;
    }

    public async Task<List<ExternalMatchDto>> GetTeamFinishedMatchesAsync(string teamId, DateTimeOffset fromDate, DateTimeOffset toDate, int numberOfItems, CancellationToken cancellationToken = default)
    {
        var footballDataMatches = await InternalGetTeamFinishedMatchesAsync(teamId, fromDate, toDate, numberOfItems, cancellationToken).ConfigureAwait(false);
        if (footballDataMatches == null)
            return [];

        var matches = FootballDataMapper.MapToExternalMatch(footballDataMatches);
        return matches;
    }

    public async Task<List<ExternalMatchDto>> GetCompetitionUpcomingMatchesAsync(string competitionId, DateTimeOffset fromDate, DateTimeOffset toDate, CancellationToken cancellationToken = default)
    {
        var footballDataMatches = await InternalGetTeamUpcomingMatchesAsync(competitionId, fromDate, toDate, cancellationToken).ConfigureAwait(false);
        if (footballDataMatches == null)
            return [];

        var matches = FootballDataMapper.MapToExternalMatch(footballDataMatches);
        return matches;
    }
    #endregion

    #region Internal API Methods
    private Task<FootballDataCompetition?> InternalGetCompetitionAsync(string competitionId, CancellationToken cancellationToken = default)
        => GetAsync<FootballDataCompetition>($"competitions/{competitionId}", cancellationToken);

    public Task<FootballDataStandingsResponse?> InternalGetCompetitionStandingsAsync(string competitionId, CancellationToken cancellationToken = default)
        => GetAsync<FootballDataStandingsResponse>($"competitions/{competitionId}/standings", cancellationToken);
    #endregion
    public async Task<IReadOnlyList<FootballDataMatch>?> InternalGetTeamUpcomingMatchesAsync(string competitionId, DateTimeOffset fromDate, DateTimeOffset toDate, CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<FootballDataMatchesResponse>($"competitions/{competitionId}/matches?status=SCHEDULED&dateFrom={fromDate:yyyy-MM-dd}&dateTo={toDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);
        var matches = response?.Matches?.ToArray();
        return matches;
    }

    private async Task<IReadOnlyList<FootballDataMatch>?> InternalGetTeamFinishedMatchesAsync(string teamId, DateTimeOffset fromDate, DateTimeOffset toDate, int numberOfItems, CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<FootballDataMatchesResponse>($"teams/{teamId}/matches?status=FINISHED&dateFrom={fromDate:yyyy-MM-dd}&dateTo={toDate:yyyy-MM-dd}&limit={numberOfItems}", cancellationToken).ConfigureAwait(false);
        var matches = response?.Matches?.ToArray();
        return matches;
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
        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await PersistProviderLogAsync(request.RequestUri!.ToString());
        return response;
    }

    private async Task PersistProviderLogAsync(string path)
    {
        var log = new ExternalProvidersLogsEntity
        {
            ProviderId = Id,
            RequestPath = path,
            RequestDate = DateTime.UtcNow
        };

        await _providerLogger.SaveLogAsync(log).ConfigureAwait(false);
        _logs.Add(log);
    }

    private async Task EnforceRateLimitAsync(CancellationToken cancellationToken)
    {
        await InitializeProviderDataAsync().ConfigureAwait(false);

        var delay = TimeSpan.Zero;

        while (true)
        {
            await RateLimitSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(6), cancellationToken).ConfigureAwait(false);
                var now = DateTime.UtcNow;

                if (IsLimitPerMinute())
                {
                    delay = (now.AddMinutes(1) - now);
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }

                break;

            }
            finally
            {
                RateLimitSemaphore.Release();
            }
        }
    }

    private bool IsLimitPerMinute()
    {
        if (_settings == null || !_settings.MaxRequestsPerMinute.HasValue)
            return false;

        var limitPerMinute = _settings.MaxRequestsPerMinute.Value;
        var requestPerMinute = _logs.Count(c => c.RequestDate > DateTime.UtcNow.AddMinutes(-1));
        return requestPerMinute >= limitPerMinute;
    }

    private async Task InitializeProviderDataAsync()
    {
        if (_logs.Count == 0)
        {
            var logs = await _providerLogger.GetLogsAsync(Id).ConfigureAwait(false);
            _logs = new ConcurrentBag<ExternalProvidersLogsEntity>(logs);
        }

        if (_settings == null)
        {
            await _providerLogger.GetProvidersEntityAsync(Id).ContinueWith(task =>
            {
                _settings = task.Result;
            }).ConfigureAwait(false);
        }
    }
}
