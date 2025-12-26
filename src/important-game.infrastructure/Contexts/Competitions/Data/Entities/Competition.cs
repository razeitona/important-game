using important_game.infrastructure.Contexts.Matches.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Competitions.Data.Entities;

[Table("competition")]
[ExcludeFromCodeCoverage]
public class Competition
{
    [Key]
    [Required]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public string? Code { get; set; }

    [Required]
    public string PrimaryColor { get; set; }

    [Required]
    public string BackgroundColor { get; set; }

    [Required]
    public double LeagueRanking { get; set; }

    [Required]
    public bool IsActive { get; set; }

    public int? TitleHolderTeamId { get; set; }
    public DateTimeOffset? LastUpdateDate { get; set; }

    // Navigation property for Fixtures in the competition
    public ICollection<Match> Fixtures { get; set; }
}
