namespace important_game.infrastructure.Contexts.Matches.Data.Entities;
public class HeadToHeadDto
{
    public DateTimeOffset MatchDateUTC { get; set; }
    public int HomeTeamId { get; set; }
    public required string HomeTeamName { get; set; }
    public int HomeTeamScore { get; set; }
    public int AwayTeamId { get; set; }
    public required string AwayTeamName { get; set; }
    public int AwayTeamScore { get; set; }
}
