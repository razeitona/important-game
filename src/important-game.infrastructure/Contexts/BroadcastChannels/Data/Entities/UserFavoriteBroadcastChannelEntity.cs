namespace important_game.infrastructure.Contexts.BroadcastChannels.Data.Entities;

public class UserFavoriteBroadcastChannelEntity
{
    public int UserId { get; set; }
    public int ChannelId { get; set; }
    public DateTime AddedAt { get; set; }
}
