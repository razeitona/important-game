using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations.Schema;

namespace important_game.infrastructure.ImportantMatch.Data.Entities
{
    [Table("competition")]
    [ExcludeFromCodeCoverage]
    public class Competition
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string PrimaryColor { get; set; }

        [Required]
        public string BackgroundColor { get; set; }

        [Required]
        public double LeagueRanking { get; set; }

        [Required]
        public bool IsActive { get; set; }



        //[ForeignKey(nameof(TitleHolderTeam))]
        public int? TitleHolderTeamId { get; set; }
        //public Team TitleHolderTeam { get; set; }

        // Navigation property for Fixtures in the competition
        public ICollection<Match> Fixtures { get; set; }
    }
}

