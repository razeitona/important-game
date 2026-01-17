using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI;

[ExcludeFromCodeCoverage]
public class TwitterIntegration : ITwitterIntegration
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TwitterIntegration> _logger;
    private readonly TwitterOptions _options;

    public TwitterIntegration(
        HttpClient httpClient,
        ILogger<TwitterIntegration> logger,
        IOptions<TwitterOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<TwitterPostResponse?> PostTweetAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("Cannot post empty tweet content");
            return null;
        }

        if (content.Length > 280)
        {
            _logger.LogWarning("Tweet content exceeds 280 characters, truncating");
            content = content[..280];
        }

        try
        {
            var url = $"{TwitterConstants.BaseUrl}{TwitterConstants.TweetsEndpoint}";
            var request = new TwitterPostRequest { Text = content };
            var jsonContent = JsonSerializer.Serialize(request);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var authHeader = GenerateOAuthHeader(HttpMethod.Post, url);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader);

            _logger.LogInformation("Posting tweet: {Content}", content[..Math.Min(50, content.Length)] + "...");

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Twitter API error: {StatusCode} - {Response}", response.StatusCode, responseContent);
                return null;
            }

            var result = JsonSerializer.Deserialize<TwitterPostResponse>(responseContent);
            _logger.LogInformation("Tweet posted successfully with ID: {TweetId}", result?.Data?.Id);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting tweet");
            return null;
        }
    }

    public async Task<bool> ValidateCredentialsAsync()
    {
        try
        {
            var url = $"{TwitterConstants.BaseUrl}{TwitterConstants.UsersEndpoint}";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            var authHeader = GenerateOAuthHeader(HttpMethod.Get, url);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Twitter credentials validation failed: {StatusCode} - {Response}",
                    response.StatusCode, responseContent);
                return false;
            }

            var result = JsonSerializer.Deserialize<TwitterUserResponse>(responseContent);
            _logger.LogInformation("Twitter credentials validated for user: @{Username}", result?.Data?.Username);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Twitter credentials");
            return false;
        }
    }

    private string GenerateOAuthHeader(HttpMethod method, string url)
    {
        var oauthNonce = GenerateNonce();
        var oauthTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var parameters = new SortedDictionary<string, string>
        {
            { "oauth_consumer_key", _options.ApiKey },
            { "oauth_nonce", oauthNonce },
            { "oauth_signature_method", "HMAC-SHA1" },
            { "oauth_timestamp", oauthTimestamp },
            { "oauth_token", _options.AccessToken },
            { "oauth_version", "1.0" }
        };

        var parameterString = string.Join("&",
            parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        var signatureBaseString = $"{method.Method.ToUpperInvariant()}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(parameterString)}";

        var signingKey = $"{Uri.EscapeDataString(_options.ApiKeySecret)}&{Uri.EscapeDataString(_options.AccessTokenSecret)}";

        var signature = GenerateSignature(signatureBaseString, signingKey);
        parameters.Add("oauth_signature", signature);

        return string.Join(", ",
            parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}=\"{Uri.EscapeDataString(kvp.Value)}\""));
    }

    private static string GenerateNonce()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");
    }

    private static string GenerateSignature(string baseString, string key)
    {
        using var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(key));
        var hash = hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString));
        return Convert.ToBase64String(hash);
    }
}
