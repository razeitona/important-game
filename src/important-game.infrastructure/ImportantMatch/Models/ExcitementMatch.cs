namespace important_game.infrastructure.ImportantMatch.Models
{

    public class ExcitementMatch
    {
        public int Id { get; set; }
        public League League { get; set; }
        public DateTimeOffset MatchDate { get; set; }
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public double ExcitementScore { get; set; }
        public Dictionary<string, double> Score { get; set; }
        public List<Fixture> HeadToHead { get; set; }
    }

    public class LiveExcitementMatch : ExcitementMatch
    {
        public double LiveExcitementScore { get; set; }
    }
}
