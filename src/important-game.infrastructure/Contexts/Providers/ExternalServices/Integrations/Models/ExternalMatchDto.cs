
namespace important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations.Models;
public class ExternalMatchDto
{
    public string Id { get; set; } = string.Empty;
    public DateTimeOffset MatchDateUtc { get; set; }
    public ExternalTeamDto HomeTeam { get; set; } = new();
    public ExternalTeamDto AwayTeam { get; set; } = new();
    public int? HomeGoals { get; set; }
    public int? AwayGoals { get; set; }
    public int? Round { get; set; }
    public ExternalCompetitionDto? Competition { get; set; }
    public ExternalSeasonDto? Season { get; set; }
}
