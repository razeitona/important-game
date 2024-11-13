namespace important_game.infrastructure.ImportantMatch.Models
{
    public class ExcitementMatchDetailDto
    {
        public int Id { get; init; }
        public LeagueDto League { get; init; }
        public DateTimeOffset MatchDate { get; init; }
        public TeamMatchDetailDto HomeTeam { get; init; }
        public TeamMatchDetailDto AwayTeam { get; init; }
        public double ExcitementScore { get; init; }
        public bool IsLive { get; set; }
        public bool IsRivalry { get; set; }
        public bool HasTitleHolder { get; set; }
        public int Favorites { get; set; }
        public List<FixtureDto> Headtohead { get; set; } = new();
        public Dictionary<string, (bool Show, double Value)> ExcitmentScoreDetail { get; set; } = new();
        public double Minutes { get; set; }
        public double LiveExcitementScore { get; set; }
        public string Description { get; set; }
    }
}
