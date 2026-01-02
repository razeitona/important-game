namespace important_game.infrastructure.Contexts.BroadcastChannels.Data.Entities;

public class BroadcastChannelEntity
{
    public int ChannelId { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
