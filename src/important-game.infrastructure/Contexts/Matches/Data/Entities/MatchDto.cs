namespace important_game.infrastructure.Contexts.Matches.Data.Entities;
public class MatchDto
{
    public int MatchId { get; set; }
    public int CompetitionId { get; set; }
    public string? CompetitionName { get; set; }
    public string? CompetitionPrimaryColor { get; set; }
    public string? CompetitionBackgroundColor { get; set; }
    public DateTimeOffset MatchDateUTC { get; set; }
    public int HomeTeamId { get; set; }
    public required string HomeTeamName { get; set; }
    public int? HomeTeamScore { get; set; }
    public int AwayTeamId { get; set; }
    public required string AwayTeamName { get; set; }
    public int? AwayTeamScore { get; set; }
    public double ExcitmentScore { get; set; }

    // Computed property - match is live if started within last 90 minutes
    public bool IsLive
    {
        get
        {
            var now = DateTimeOffset.UtcNow;
            var matchEnd = MatchDateUTC.AddMinutes(120);
            return now >= MatchDateUTC && now <= matchEnd;
        }
    }
}
