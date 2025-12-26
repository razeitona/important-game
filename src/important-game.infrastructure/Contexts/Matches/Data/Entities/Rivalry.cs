using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations.Schema;

namespace important_game.infrastructure.Contexts.Matches.Data.Entities
{
    [Table("rivalry")]
    [ExcludeFromCodeCoverage]
    public class Rivalry
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int TeamOneId { get; set; }
        public int TeamTwoId { get; set; }

        [Required]
        public double RivarlyValue { get; set; }
    }
}
