namespace important_game.infrastructure.ImportantMatch.Models.Processors
{
    public class LeagueStanding
    {
        public int LeagueId { get; set; }
        public int CurrentRound { get; set; }
        public int TotalRounds { get; set; }
        public List<Standing> Standings { get; set; }
    }

    public class Standing
    {
        public TeamInfo Team { get; set; }
        public int Position { get; set; }
        public int Matches { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int Points { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
    }



}
