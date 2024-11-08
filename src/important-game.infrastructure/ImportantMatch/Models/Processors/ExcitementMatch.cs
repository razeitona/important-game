namespace important_game.infrastructure.ImportantMatch.Models.Processors
{
    public class ExcitementMatch
    {
        public int Id { get; set; }
        public int LeagueId { get; set; }
        public DateTimeOffset MatchDate { get; set; }
        public TeamInfo HomeTeam { get; set; }
        public TeamInfo AwayTeam { get; set; }
        public double ExcitementScore { get; set; }
        public Dictionary<string, double> Score { get; set; }
        public double LiveExcitementScore { get; set; }
        public List<Fixture> HeadToHead { get; set; }
    }
}
