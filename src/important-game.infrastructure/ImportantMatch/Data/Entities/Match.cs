using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace important_game.infrastructure.ImportantMatch.Data.Entities
{
    [Table("match")]
    public class Match
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Competition))]
        public int CompetitionId { get; set; }
        public Competition Competition { get; set; }

        [Required]
        public DateTime MatchDateUTC { get; set; }



        [ForeignKey(nameof(HomeTeam))]
        public int HomeTeamId { get; set; }
        public Team HomeTeam { get; set; }
        public int HomeTeamPosition { get; set; }


        [ForeignKey(nameof(AwayTeam))]
        public int AwayTeamId { get; set; }
        public Team AwayTeam { get; set; }
        public int AwayTeamPosition { get; set; }

        public int HomeScore { get; set; }
        public int AwayScore { get; set; }

        public string? HomeForm { get; set; }
        public string? AwayForm { get; set; }
        public MatchStatus MatchStatus { get; set; }

        [Required]
        public double ExcitmentScore { get; set; }

        public double CompetitionScore { get; set; }
        public double FixtureScore { get; set; }
        public double FormScore { get; set; }
        public double GoalsScore { get; set; }
        public double CompetitionStandingScore { get; set; }
        public double HeadToHeadScore { get; set; }
        public double RivalryScore { get; set; }
        public double TitleHolderScore { get; set; }

        [Required]
        public DateTime UpdatedDateUTC { get; set; }

        public ICollection<LiveMatch> LiveMatches { get; set; }
        public ICollection<Headtohead> HeadToHead { get; set; }
    }
}
