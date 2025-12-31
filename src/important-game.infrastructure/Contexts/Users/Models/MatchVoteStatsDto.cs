namespace important_game.infrastructure.Contexts.Users.Models;

public class MatchVoteStatsDto
{
    public int MatchId { get; set; }
    public int TotalVotes { get; set; }
    public int VoteScore { get; set; }
}
