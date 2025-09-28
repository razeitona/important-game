using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations.Schema;

namespace important_game.infrastructure.ImportantMatch.Data.Entities
{
    [Table("rivalry")]
    [ExcludeFromCodeCoverage]
    public class Rivalry
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //[ForeignKey(nameof(TeamOne))]
        public int TeamOneId { get; set; }
        //public Team TeamOne { get; set; }


        //[ForeignKey(nameof(TeamTwo))]
        public int TeamTwoId { get; set; }
        //public Team TeamTwo { get; set; }

        [Required]
        public double RivarlyValue { get; set; }
    }
}

