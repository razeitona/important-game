namespace important_game.infrastructure.Contexts.BroadcastChannels.Models;

public class CountryWithChannelsViewModel
{
    public required string CountryCode { get; set; }
    public required string CountryName { get; set; }
    public List<BroadcastChannelViewModel> Channels { get; set; } = new();
}
