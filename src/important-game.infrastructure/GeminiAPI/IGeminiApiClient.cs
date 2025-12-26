using important_game.infrastructure.GeminiAPI.Models;

namespace important_game.infrastructure.GeminiAPI
{
    /// <summary>
    /// Interface for Gemini API client.
    /// Handles all communication with Google's Gemini API for enriching competition data.
    /// </summary>
    public interface IGeminiApiClient
    {
        /// <summary>
        /// Fetch enriched competition data from Gemini API.
        /// Includes calendar, standings, and team statistics as requested.
        /// </summary>
        /// <param name="request">Request containing competition and team data requirements.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Competition response with requested data.</returns>
        /// <exception cref="HttpRequestException">Thrown when API request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when response cannot be parsed.</exception>
        Task<GeminiCompetitionResponse> FetchCompetitionDataAsync(
            GeminiCompetitionRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if API rate limit allows new requests.
        /// </summary>
        /// <returns>True if request can be made within rate limits.</returns>
        bool CanMakeRequest();

        /// <summary>
        /// Get remaining requests in current rate limit window.
        /// </summary>
        /// <returns>Number of requests remaining.</returns>
        int GetRemainingRequests();
    }
}
