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
    public int AwayTeamId { get; set; }
    public required string AwayTeamName { get; set; }
    public double ExcitmentScore { get; set; }
}
