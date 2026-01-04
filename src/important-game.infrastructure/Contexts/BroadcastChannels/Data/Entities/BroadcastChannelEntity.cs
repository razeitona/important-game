namespace important_game.infrastructure.Contexts.BroadcastChannels.Data.Entities;

public class BroadcastChannelEntity
{
    public int ChannelId { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string CountryCode { get; set; }
}
