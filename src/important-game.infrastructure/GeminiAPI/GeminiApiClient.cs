using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using important_game.infrastructure.GeminiAPI.Models;

namespace important_game.infrastructure.GeminiAPI
{
    /// <summary>
    /// Gemini API client implementation using HttpClient.
    /// Handles rate limiting and JSON parsing for Gemini API responses.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GeminiApiClient : IGeminiApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiApiClient> _logger;
        private readonly string _apiKey;
        private readonly int _rateLimitPerMinute;
        private readonly int _requestTimeoutSeconds;
        
        private DateTime _windowStart = DateTime.UtcNow;
        private int _requestsInWindow = 0;

        private const string GeminiApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

        public GeminiApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _apiKey = _configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini:ApiKey not configured");
            _rateLimitPerMinute = _configuration.GetValue("Gemini:RateLimitPerMinute", 15);
            _requestTimeoutSeconds = _configuration.GetValue("Gemini:RequestTimeoutSeconds", 30);
        }

        public async Task<GeminiCompetitionResponse> FetchCompetitionDataAsync(
            GeminiCompetitionRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!CanMakeRequest())
            {
                throw new InvalidOperationException($"Rate limit exceeded. Maximum {_rateLimitPerMinute} requests per minute.");
            }

            try
            {
                var prompt = BuildPrompt(request);
                var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(_requestTimeoutSeconds));

                var response = await _httpClient.PostAsync(
                    $"{GeminiApiBaseUrl}?key={_apiKey}",
                    jsonContent,
                    cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API request failed with status {StatusCode}", response.StatusCode);
                    throw new HttpRequestException($"Gemini API returned {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync(cts.Token);
                var parsedResponse = ParseGeminiResponse(responseContent);
                
                IncrementRequestCount();

                return parsedResponse;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Gemini API request timeout for competition {CompetitionId}", request.CompetitionId);
                throw new HttpRequestException($"Gemini API timeout after {_requestTimeoutSeconds} seconds", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Gemini API HTTP error for competition {CompetitionId}", request.CompetitionId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Gemini API for competition {CompetitionId}", request.CompetitionId);
                throw;
            }
        }

        public bool CanMakeRequest()
        {
            // Reset window if a minute has passed
            if ((DateTime.UtcNow - _windowStart).TotalSeconds >= 60)
            {
                _windowStart = DateTime.UtcNow;
                _requestsInWindow = 0;
            }

            return _requestsInWindow < _rateLimitPerMinute;
        }

        public int GetRemainingRequests()
        {
            if ((DateTime.UtcNow - _windowStart).TotalSeconds >= 60)
            {
                return _rateLimitPerMinute;
            }

            return Math.Max(0, _rateLimitPerMinute - _requestsInWindow);
        }

        private void IncrementRequestCount()
        {
            _requestsInWindow++;
        }

        private string BuildPrompt(GeminiCompetitionRequest request)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"You are a football/soccer data expert. Provide accurate JSON-formatted responses.");
            sb.AppendLine($"Competition: {request.CompetitionName} (ID: {request.CompetitionId})");
            sb.AppendLine();

            if (request.RequestCalendar)
            {
                sb.AppendLine("CALENDAR REQUEST:");
                sb.AppendLine("Provide upcoming fixtures for the current season in JSON format:");
                sb.AppendLine(@"{ ""fixtures"": [ { ""id"": 123, ""homeTeamId"": 456, ""homeTeamName"": ""Team A"", ""awayTeamId"": 789, ""awayTeamName"": ""Team B"", ""matchDateUTC"": ""2024-12-01T15:00:00Z"" } ] }");
                sb.AppendLine();
            }

            if (request.RequestTable)
            {
                sb.AppendLine("STANDINGS REQUEST:");
                sb.AppendLine("Provide current league standings in JSON format:");
                sb.AppendLine(@"{ ""standings"": [ { ""teamId"": 456, ""teamName"": ""Team A"", ""position"": 1, ""points"": 30, ""matches"": 10, ""wins"": 9, ""draws"": 1, ""losses"": 0, ""goalsFor"": 25, ""goalsAgainst"": 5 } ] }");
                sb.AppendLine();
            }

            if (request.TeamRequests.Count > 0)
            {
                sb.AppendLine("TEAM STATISTICS REQUEST (Last 5 matches):");
                sb.AppendLine(@"{ ""teamStats"": [ { ""teamId"": 456, ""teamName"": ""Team A"", ""goalsFor5"": 10, ""goalsAgainst5"": 2, ""wins5"": 4, ""draws5"": 1, ""losses5"": 0 } ] }");
                sb.AppendLine("Teams to analyze:");
                foreach (var team in request.TeamRequests)
                {
                    sb.AppendLine($"- {team.TeamName} (ID: {team.TeamId}) - Last {team.LastDays} matches");
                }
                sb.AppendLine();
            }

            sb.AppendLine("Respond ONLY with valid JSON. No additional text.");

            return sb.ToString();
        }

        private GeminiCompetitionResponse ParseGeminiResponse(string responseContent)
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(responseContent);
                var root = jsonDoc.RootElement;

                // Extract text content from Gemini response
                string textContent = string.Empty;
                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var content) &&
                        content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        var firstPart = parts[0];
                        if (firstPart.TryGetProperty("text", out var text))
                        {
                            textContent = text.GetString() ?? string.Empty;
                        }
                    }
                }

                if (string.IsNullOrEmpty(textContent))
                {
                    throw new InvalidOperationException("No text content in Gemini response");
                }

                // Parse JSON from text content
                var parsedData = JsonSerializer.Deserialize<JsonElement>(textContent);
                var response = new GeminiCompetitionResponse();

                if (parsedData.TryGetProperty("fixtures", out var fixtures))
                {
                    response.Calendar = ParseCalendar(fixtures);
                }

                if (parsedData.TryGetProperty("standings", out var standings))
                {
                    response.Standings = ParseStandings(standings);
                }

                if (parsedData.TryGetProperty("teamStats", out var teamStats))
                {
                    response.TeamStatistics = ParseTeamStatistics(teamStats);
                }

                return response;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini API response");
                throw new InvalidOperationException("Invalid JSON in Gemini API response", ex);
            }
        }

        private GeminiCalendar ParseCalendar(JsonElement fixturesElement)
        {
            var calendar = new GeminiCalendar();

            if (fixturesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var fixture in fixturesElement.EnumerateArray())
                {
                    calendar.Fixtures.Add(new GeminiFixture
                    {
                        Id = fixture.GetProperty("id").GetInt32(),
                        HomeTeamId = fixture.GetProperty("homeTeamId").GetInt32(),
                        HomeTeamName = fixture.GetProperty("homeTeamName").GetString() ?? string.Empty,
                        AwayTeamId = fixture.GetProperty("awayTeamId").GetInt32(),
                        AwayTeamName = fixture.GetProperty("awayTeamName").GetString() ?? string.Empty,
                        MatchDateUTC = DateTime.Parse(fixture.GetProperty("matchDateUTC").GetString() ?? DateTime.UtcNow.ToString("O"))
                    });
                }
            }

            return calendar;
        }

        private GeminiStandings ParseStandings(JsonElement standingsElement)
        {
            var standings = new GeminiStandings();

            if (standingsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var team in standingsElement.EnumerateArray())
                {
                    standings.Teams.Add(new GeminiTeamStanding
                    {
                        TeamId = team.GetProperty("teamId").GetInt32(),
                        TeamName = team.GetProperty("teamName").GetString() ?? string.Empty,
                        Position = team.GetProperty("position").GetInt32(),
                        Points = team.GetProperty("points").GetInt32(),
                        Matches = team.GetProperty("matches").GetInt32(),
                        Wins = team.GetProperty("wins").GetInt32(),
                        Draws = team.GetProperty("draws").GetInt32(),
                        Losses = team.GetProperty("losses").GetInt32(),
                        GoalsFor = team.GetProperty("goalsFor").GetInt32(),
                        GoalsAgainst = team.GetProperty("goalsAgainst").GetInt32()
                    });
                }
            }

            return standings;
        }

        private List<GeminiTeamStatistics> ParseTeamStatistics(JsonElement statsElement)
        {
            var stats = new List<GeminiTeamStatistics>();

            if (statsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var team in statsElement.EnumerateArray())
                {
                    stats.Add(new GeminiTeamStatistics
                    {
                        TeamId = team.GetProperty("teamId").GetInt32(),
                        TeamName = team.GetProperty("teamName").GetString() ?? string.Empty,
                        GoalsFor5 = team.GetProperty("goalsFor5").GetInt32(),
                        GoalsAgainst5 = team.GetProperty("goalsAgainst5").GetInt32(),
                        Wins5 = team.GetProperty("wins5").GetInt32(),
                        Draws5 = team.GetProperty("draws5").GetInt32(),
                        Losses5 = team.GetProperty("losses5").GetInt32()
                    });
                }
            }

            return stats;
        }
    }
}
