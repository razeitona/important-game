namespace important_game.infrastructure.Contexts.BroadcastChannels.Models;

public class MatchBroadcastViewModel
{
    public int MatchId { get; set; }
    public int ChannelId { get; set; }
    public required string ChannelName { get; set; }
    public required string ChannelCode { get; set; }
    public required string CountryCode { get; set; }
    public required string CountryName { get; set; }
}
