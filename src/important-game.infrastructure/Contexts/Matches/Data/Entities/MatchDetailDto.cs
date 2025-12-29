namespace important_game.infrastructure.Contexts.Matches.Data.Entities;
public class MatchDetailDto
{
    public int MatchId { get; set; }
    public int CompetitionId { get; set; }
    public int? SeasonId { get; set; }
    public string? CompetitionName { get; set; }
    public string? CompetitionPrimaryColor { get; set; }
    public string? CompetitionBackgroundColor { get; set; }
    public DateTimeOffset MatchDateUTC { get; set; }
    public int HomeTeamId { get; set; }
    public required string HomeTeamName { get; set; }
    public string? HomeTeamForm { get; set; }
    public int? HomeTeamTablePosition { get; set; }
    public int AwayTeamId { get; set; }
    public required string AwayTeamName { get; set; }
    public string? AwayTeamForm { get; set; }
    public int? AwayTeamTablePosition { get; set; }
    public double ExcitmentScore { get; set; }
    public double CompetitionScore { get; set; }
    public double CompetitionStandingScore { get; set; }
    public double FixtureScore { get; set; }
    public double FormScore { get; set; }
    public double GoalsScore { get; set; }
    public double HeadToHeadScore { get; set; }
    public double RivalryScore { get; set; }
    public double TitleHolderScore { get; set; }

}
