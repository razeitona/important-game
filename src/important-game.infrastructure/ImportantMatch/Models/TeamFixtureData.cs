﻿namespace important_game.infrastructure.ImportantMatch.Models
{
    public class TeamFixtureData
    {
        public int AmountOfGames { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public List<string> FixtureResult { get; set; } = new List<string>();
    }
}
