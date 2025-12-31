namespace important_game.infrastructure.Contexts.Users.Models;

public class MatchVoteDto
{
    public int UserId { get; set; }
    public int MatchId { get; set; }
    public int VoteType { get; set; }
    public DateTime VotedAt { get; set; }
}
