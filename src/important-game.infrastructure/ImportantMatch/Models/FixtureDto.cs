namespace important_game.infrastructure.ImportantMatch.Models
{
    public class FixtureDto
    {
        public DateTimeOffset MatchDate { get; set; }
        public string HomeTeamName { get; set; }
        public int HomeTeamScore { get; set; }
        public string AwayTeamName { get; set; }
        public int AwayTeamScore { get; set; }
    }
}
