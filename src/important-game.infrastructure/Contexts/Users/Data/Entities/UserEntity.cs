namespace important_game.infrastructure.Contexts.Users.Data.Entities;

public class UserEntity
{
    public int UserId { get; set; }
    public required string GoogleId { get; set; }
    public required string Email { get; set; }
    public string? Name { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string PreferredTimezone { get; set; } = "UTC";
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
