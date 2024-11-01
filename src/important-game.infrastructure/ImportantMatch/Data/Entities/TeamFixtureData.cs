using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.infrastructure.ImportantMatch.Data.Entities
{
    public class TeamFixtureData
    {
        public int AmountOfGames { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public List<MatchResultType> FixtureResult { get; set; } = new List<MatchResultType>();
    }
}
