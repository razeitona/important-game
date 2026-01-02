using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Enums;

namespace important_game.infrastructure.Contexts.Matches.Models;
public class MatchDetailViewModel
{
    public int MatchId { get; set; }
    public int CompetitionId { get; set; }
    public string? CompetitionName { get; set; }
    public string? CompetitionPrimaryColor { get; set; }
    public string? CompetitionBackgroundColor { get; set; }
    public DateTimeOffset MatchDateUTC { get; set; }
    public int? TitleHolderId { get; set; }
    public int HomeTeamId { get; set; }
    public string HomeTeamName { get; set; }
    public List<MatchResultType> HomeTeamForm { get; set; } = [];
    public int? HomeTeamTablePosition { get; set; }
    public int AwayTeamId { get; set; }
    public string AwayTeamName { get; set; }
    public List<MatchResultType> AwayTeamForm { get; set; } = [];
    public int? AwayTeamTablePosition { get; set; }
    public double ExcitmentScore { get; set; }
    public bool IsRivalry { get; set; } = false;
    public bool HasTitleHolder { get; set; } = false;
    public bool IsLive { get; set; } = false;
    public Dictionary<string, (bool Show, double Value)> ExcitmentScoreDetail { get; set; } = new();
    public string? Description { get; set; }
    public List<HeadToHeadDto> HeadToHead { get; set; } = new();
}
