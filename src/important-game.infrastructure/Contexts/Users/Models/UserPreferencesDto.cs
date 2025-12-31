namespace important_game.infrastructure.Contexts.Users.Models;

public class UserPreferencesDto
{
    public string PreferredTimezone { get; set; } = "UTC";
    public string? Name { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
