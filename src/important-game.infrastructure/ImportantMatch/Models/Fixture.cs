namespace important_game.infrastructure.ImportantMatch.Models
{
    public class UpcomingFixture : Fixture
    {
        public List<Fixture> HeadToHead { get; set; }
    }

    public class Fixture
    {
        public DateTimeOffset MatchDate { get; set; }
        public Team HomeTeam { get; set; }
        public int HomeTeamScore { get; set; }
        public Team AwayTeam { get; set; }
        public int AwayTeamScore { get; set; }
    }
}
