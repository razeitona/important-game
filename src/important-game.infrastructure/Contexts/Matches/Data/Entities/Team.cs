using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations.Schema;

namespace important_game.infrastructure.Contexts.Matches.Data.Entities
{
    [Table("team")]
    [ExcludeFromCodeCoverage]
    public class Team
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Match> HomeFixtures { get; set; }
        public ICollection<Match> AwayFixtures { get; set; }
        public ICollection<Headtohead> HomeHeadToHead { get; set; }
        public ICollection<Headtohead> AwayHeadToHead { get; set; }
    }
}
