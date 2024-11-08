﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace important_game.infrastructure.ImportantMatch.Data.Entities
{
    [Table("team")]
    public class Team
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Match> HomeFixtures { get; set; }
        public ICollection<Match> AwayFixtures { get; set; }

        //public ICollection<Rivalry> TeamOneRivalries { get; set; }
        //public ICollection<Rivalry> TeamTwoRivalries { get; set; }
        public ICollection<Headtohead> HomeHeadToHead { get; set; }
        public ICollection<Headtohead> AwayHeadToHead { get; set; }
    }
}
