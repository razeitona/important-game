namespace important_game.infrastructure.ImportantMatch.Models.Processors
{

    public abstract class TeamBaseInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TeamInfo : TeamBaseInfo
    {
        public int Position { get; set; }
        public int H2hWins { get; set; }
        public int Goals { get; set; }
        public TeamFixtureData LastFixtures { get; set; }
        public bool IsTitleHolder { get; set; }
    }

    public class TeamTitleHolder : TeamBaseInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
