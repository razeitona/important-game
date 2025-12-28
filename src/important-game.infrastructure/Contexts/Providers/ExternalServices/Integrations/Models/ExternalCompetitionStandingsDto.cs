namespace important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations.Models;

public class ExternalCompetitionStandingsDto
{
    //public ExternalCompetitionDto? Competition { get; set; }
    //public ExternalSeasonDto? Season { get; set; }
    public List<ExternalCompetitionStandingRowDto> Standings { get; set; } = [];
}

public class ExternalCompetitionStandingRowDto
{
    public int Position { get; set; }
    public ExternalTeamDto Team { get; set; } = new();
    public int PlayedGames { get; set; }
    public int Won { get; set; }
    public int Draw { get; set; }
    public int Lost { get; set; }
    public int Points { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
}
