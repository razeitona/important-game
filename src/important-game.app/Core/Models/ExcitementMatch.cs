namespace important_game.ui.Core.Models
{

    public class ExcitementMatch
    {
        public League League { get; set; }
        public DateTimeOffset MatchDate { get; set; }
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public double ExcitementScore { get; set; }

        public Dictionary<string, double> Score { get; set; }
    }
}
