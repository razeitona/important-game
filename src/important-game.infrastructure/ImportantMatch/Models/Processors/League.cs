namespace important_game.infrastructure.ImportantMatch.Models.Processors
{
    public class League
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public LeagueSeason CurrentSeason { get; set; }
        public TeamTitleHolder TitleHolder { get; set; }
        public double Ranking { get; set; }
    }

    public class LeagueSeason
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Round { get; set; }
    }
}