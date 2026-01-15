namespace important_game.infrastructure.Contexts.ScoreCalculator.Utils;
internal class CalculatorCoeficients
{
    internal static double CompetitionCoef(bool isLateStage) => isLateStage ? 0.15d : 0.05d;
    internal static double FixtureCoef(bool isLateStage) => isLateStage ? 0.10d : 0.25d;
    internal static double TeamFormCoef(bool isLateStage) => isLateStage ? 0.10d : 0.05d;
    internal static double TeamGoalsCoef(bool isLateStage) => isLateStage ? 0.15d : 0.2d;
    internal static double TableRankCoef(bool isLateStage) => isLateStage ? 0.15d : 0.33d;
    internal static double HeadToHeadCoef(bool isLateStage) => isLateStage ? 0.10d : 0.05d;
    internal static double TitleHolderCoef(bool isLateStage) => isLateStage ? 0.10d : 0.05d;
    internal static double RivalryCoef(bool isLateStage) => isLateStage ? 0.15d : 0.02d;

    internal static double ScoreLineCoef => 0.25d;
    internal static double XGoalsCoef => 0.20d;
    internal static double TotalFoulsCoef => 0.10d;
    internal static double TotalCardsCoef => 0.15d;
    internal static double PosessionCoef => 0.10d;
    internal static double BigChancesCoef => 0.20d;
}
