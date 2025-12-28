namespace important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations.Models;
public class ExternalCompetitionDto
{
    public string Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Type { get; set; }
    public ExternalSeasonDto? CurrentSeason { get; set; }
    public int? NumberOfAvailableSeasons { get; set; }
}

public class ExternalSeasonDto
{
    public string Id { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? CurrentMatchday { get; set; }
    public ExternalTeamDto? Winner { get; set; }
}