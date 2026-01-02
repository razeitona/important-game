namespace important_game.infrastructure.Contexts.BroadcastChannels.Models;

public class BroadcastChannelViewModel
{
    public int ChannelId { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public bool IsFavorite { get; set; }
}
