using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace important_game.infrastructure.ImportantMatch.Data.Entities
{
    [Table("live_match")]
    public class LiveMatch
    {
        [Key]
        public int Id { get; set; }
        public DateTime RegisteredDate { get; set; }


        [ForeignKey(nameof(Match))]
        public int MatchId { get; set; }
        public Match Match { get; set; }


        [Required]
        public int HomeScore { get; set; }

        [Required]
        public int AwayScore { get; set; }

        [Required]
        public int Minutes { get; set; }

        [Required]
        public double ExcitmentScore { get; set; }

        public double ScoreLineScore { get; set; }
        public double ShotTargetScore { get; set; }
        public double XGoalsScore { get; set; }
        public double TotalFoulsScore { get; set; }
        public double TotalCardsScore { get; set; }
        public double PossesionScore { get; set; }
        public double BigChancesScore { get; set; }
    }
}
