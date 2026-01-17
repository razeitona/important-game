namespace important_game.infrastructure.Contexts.Providers.ExternalServices.TwitterAPI;

public class TwitterOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiKeySecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string AccessTokenSecret { get; set; } = string.Empty;
    public double MinExcitementScoreToPost { get; set; } = 0.7;
    public int HoursBeforeMatchToPost { get; set; } = 2;
    public int MaxPostsPerMonth { get; set; } = 100;
    public bool Enabled { get; set; } = false;
}
