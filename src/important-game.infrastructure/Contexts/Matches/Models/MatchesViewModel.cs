using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Contexts.Matches.Models;
public class MatchesViewModel
{
    public List<CompetitionDto> Competitions { get; set; } = [];
    public List<MatchDto> Matches { get; set; } = [];
}
