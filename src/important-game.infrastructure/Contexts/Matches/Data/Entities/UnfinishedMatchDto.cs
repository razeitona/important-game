namespace important_game.infrastructure.Contexts.Matches.Data.Entities;
public class UnfinishedMatchDto
{
    public int MatchId { get; set; }
    public int CompetitionId { get; set; }
    public int SeasonId { get; set; }
    public int? TitleHolderId { get; set; }
    public double LeagueRanking { get; set; }
    public DateTimeOffset MatchDateUTC { get; set; }
    public int Round { get; set; }
    public int NumberOfRounds { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public DateTimeOffset UpdatedDateUTC { get; set; }
}
