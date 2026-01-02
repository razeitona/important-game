using important_game.infrastructure.Contexts.Matches.Enums;

namespace important_game.infrastructure.Contexts.ScoreCalculator.Models;
internal class TeamMatchesStatsDto
{
    public int Matches { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public List<MatchResultType> Form { get; set; } = [];
    public int HeadToHeadMatches { get; set; }
    public int HeadToHeadWins { get; set; }
    public int HeadToHeadDraws { get; set; }
    public int HeadToHeadLosses { get; set; }
    public int HeadToHeadGoalsFor { get; set; }
    public int HeadToHeadGoalsAgainst { get; set; }
}
