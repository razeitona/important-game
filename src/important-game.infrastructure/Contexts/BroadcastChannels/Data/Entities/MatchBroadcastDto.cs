namespace important_game.infrastructure.Contexts.BroadcastChannels.Data.Entities;

/// <summary>
/// DTO for match broadcast information including channel and country details
/// </summary>
public class MatchBroadcastDto
{
    public int MatchId { get; set; }
    public int ChannelId { get; set; }
    public required string ChannelName { get; set; }
    public required string ChannelCode { get; set; }
    public required string CountryCode { get; set; }
    public required string CountryName { get; set; }
}
