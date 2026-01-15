using System.Text.Json.Serialization;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Models.SofaScoreDto
{
    /// <summary>
    /// Response model for the live football events endpoint.
    /// Endpoint: /api/v1/sport/football/events/live
    /// </summary>
    public record SSLiveEventsResponse(
        [property: JsonPropertyName("events")] IReadOnlyList<SSEvent> Events
    );
}
