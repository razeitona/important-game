using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations.Schema;

namespace important_game.infrastructure.ImportantMatch.Data.Entities
{
    [Table("headtohead")]
    [ExcludeFromCodeCoverage]
    public class Headtohead
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Match))]
        public int MatchId { get; set; }
        public Match Match { get; set; }

        [Required]
        public DateTime MatchDateUTC { get; set; }


        [ForeignKey(nameof(HomeTeam))]
        public int HomeTeamId { get; set; }
        public Team HomeTeam { get; set; }


        [ForeignKey(nameof(AwayTeam))]
        public int AwayTeamId { get; set; }
        public Team AwayTeam { get; set; }

        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }
    }
}

