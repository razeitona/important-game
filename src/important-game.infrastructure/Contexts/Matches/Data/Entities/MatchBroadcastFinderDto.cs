namespace important_game.infrastructure.Contexts.Matches.Data.Entities;
public class MatchBroadcastFinderDto
{
    public int MatchId { get; set; }
    public DateTimeOffset MatchDateUTC { get; set; }
    public string HomeTeamName { get; set; }
    public string AwayTeamName { get; set; }
}
