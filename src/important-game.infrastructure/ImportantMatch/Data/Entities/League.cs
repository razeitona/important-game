namespace important_game.infrastructure.ImportantMatch.Data.Entities
{
    public class League
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public LeagueSeason CurrentSeason { get; set; }
        public Team TitleHolder { get; set; }
        public string PrimaryColor { get; set; }
        public string BackgroundColor { get; set; }
        public double LeagueRanking { get; set; }
    }

    public class LeagueSeason
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Round { get; set; }
    }


}
