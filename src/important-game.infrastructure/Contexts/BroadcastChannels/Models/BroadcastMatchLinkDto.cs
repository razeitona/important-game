namespace important_game.infrastructure.Contexts.BroadcastChannels.Models;

public class BroadcastMatchLinkDto
{
    public int MatchId { get; set; }
    public string ChannelName { get; set; }
    public string MappedChannelCode { get; set; }
    public double MatchConfidenceScore { get; set; } // 0.0 to 1.0
}