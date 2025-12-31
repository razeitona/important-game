namespace important_game.infrastructure.Contexts.Users.Data.Entities;

public class MatchVoteEntity
{
    public int UserId { get; set; }
    public int MatchId { get; set; }
    public int VoteType { get; set; } = 1;
    public DateTime VotedAt { get; set; }
}
