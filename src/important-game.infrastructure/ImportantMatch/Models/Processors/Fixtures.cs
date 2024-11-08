namespace important_game.infrastructure.ImportantMatch.Models.Processors
{
    public class UpcomingFixture : Fixture
    {
        public List<Fixture> HeadToHead { get; set; }
    }

    public class Fixture
    {
        public int Id { get; set; }
        public DateTimeOffset MatchDate { get; set; }
        public TeamInfo HomeTeam { get; set; }
        public int HomeTeamScore { get; set; }
        public TeamInfo AwayTeam { get; set; }
        public int AwayTeamScore { get; set; }
    }

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
