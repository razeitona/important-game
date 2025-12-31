namespace important_game.infrastructure.Contexts.Users.Data.Entities;

public class UserFavoriteMatchEntity
{
    public int UserId { get; set; }
    public int MatchId { get; set; }
    public DateTime AddedAt { get; set; }
}
