namespace important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Models.SofaScoreDto
{
    public class SSPlayerStats
    {
        public double bigChangeMissed { get; set; }
        public double bigChangeCreated { get; set; }

        public int Id { get; set; }
        public string Name { get; set; }

        public string goalConversion { get; set; }
        public double headedGoals { get; set; }
        public string penalties { get; set; }
        public string penaltiesConversion { get; set; }
        public string setPieceGoals { get; set; }
        public string setPiecesConversion { get; set; }
        public string goalsInsideBox { get; set; }
        public string goalsOutsideBox { get; set; }
        public double leftFootGoals { get; set; }
        public double rightFootGoals { get; set; }
        public double penaltyWon { get; set; }
        public double matchesTotal { get; set; }
        public double matchesStarting { get; set; }
        public double minutesPerGame { get; set; }
        public string groupName { get; set; }
        public double goals { get; set; }
        public string goalsFrequency { get; set; }
        public double goalsAverage { get; set; }
        public double totalShotsPerGame { get; set; }
        public double? bigChanceMissed { get; set; }
        public double assists { get; set; }
        public double touches { get; set; }
        public string accuratePassesPerGame { get; set; }
        public double? bigChanceCreated { get; set; }
        public double keyPasses { get; set; }
        public string successfulLongPasses { get; set; }
        public string accurateChippedPasses { get; set; }
        public string successfulPassesOwnHalf { get; set; }
        public string successfulPassesOppositionHalf { get; set; }
        public string successfulCrossesAndCorners { get; set; }
        public double cleanSheets { get; set; }
        public double interceptionsPerGame { get; set; }
        public string tacklesWonPerGame { get; set; }
        public double possessionWonFinalThird { get; set; }
        public double challengesLostPerGame { get; set; }
        public double totalClearancesPerGame { get; set; }
        public double errorLeadToAShot { get; set; }
        public double errorLeadToaGoal { get; set; }
        public double penaltiesConceded { get; set; }
        public string successfulDribblesPerGame { get; set; }
        public string duelsWonPerGame { get; set; }
        public string groundDuelsWonPerGame { get; set; }
        public string aerialDuelsWonPerGame { get; set; }
        public double possessionLost { get; set; }
        public double fouls { get; set; }
        public double wasFouled { get; set; }
        public double offsides { get; set; }
        public double yellowCards { get; set; }
        public double yellowRedCards { get; set; }
        public double redCards { get; set; }
    }
}
