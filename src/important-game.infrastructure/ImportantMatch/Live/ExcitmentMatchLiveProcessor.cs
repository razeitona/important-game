using important_game.infrastructure.ImportantMatch.Data.Entities;
using important_game.infrastructure.LeagueProcessors;

namespace important_game.infrastructure.ImportantMatch.Live
{
    internal class ExcitmentMatchLiveProcessor(ILeagueProcessor leagueProcessor) : IExcitmentMatchLiveProcessor
    {
        private const string ALL = "ALL";
        private const string MATCH_OVERVIEW = "Match overview";
        private const string TOTAL_SHOTS_GOAL = "totalShotsOnGoal";
        private const string SHOTS_ON_GOAL = "shotsOnGoal";
        private const string XGOALS = "expectedGoals";
        private const string FOULS = "fouls";
        private const string YELLOW_CARDS = "yellowCards";
        private const string RED_CARDS = "redCards";
        private const string BALL_POSSESSION = "ballPossession";
        private const string BIG_CHANCE_CREATED = "bigChanceCreated";

        public async Task<LiveMatch?> ProcessLiveMatchData(Match match)
        {
            var matchLiveData = await leagueProcessor.GetEventStatisticsAsync(match.Id.ToString());

            if (matchLiveData == null)
                return null;

            var eventInfo = await leagueProcessor.GetEventInformationAsync(match.Id.ToString());
            if (eventInfo == null) return null;

            var gameTime = eventInfo.Status.GetGameTime();

            var scoreLineCoef = 0.25d;
            var shotTargetCoef = 0.15d;
            var xGoalsCoef = 0.15d;
            var foulsCoef = 0.1d;
            var cardsCoef = 0.1d;
            var possessionCoef = 0.1d;
            var bigChangesCoef = 0.15d;


            //Score line
            //--------
            var scoreGameDif = Math.Abs(eventInfo.HomeTeamScore - eventInfo.AwayTeamScore);
            var scoreLineData = 1d - (scoreGameDif / 5d);
            var scoreLineValue = scoreLineData * scoreLineCoef;

            //Shots
            //---------
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, TOTAL_SHOTS_GOAL, out double homeTotalShots);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, TOTAL_SHOTS_GOAL, out double awayTotalShots);
            var totalShots = homeTotalShots + awayTotalShots;
            var totalShotsPerGameTime = (totalShots / gameTime);
            var totalShotsPer10Max = (totalShotsPerGameTime / 10d);

            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, SHOTS_ON_GOAL, out double homeShotsOnGoal);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, SHOTS_ON_GOAL, out double awayShotsOnGoal);
            var shotsOnTarget = homeShotsOnGoal + awayTotalShots;
            var shotsOnTargetAverage = shotsOnTarget / totalShots;

            var shotTargetValue = (totalShotsPer10Max + shotsOnTargetAverage) * shotTargetCoef;

            //Expected Goals
            //-----
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, XGOALS, out double xGoalsHome);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, XGOALS, out double xGoalsAway);

            var totalxGoals = xGoalsHome + xGoalsAway;
            var xGoalsValue = (totalxGoals / 3d) * xGoalsCoef;


            //Fouls
            //------
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, FOULS, out double homeFouls);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, FOULS, out double awayFouls);
            var totalFouls = homeFouls + awayFouls;
            var totalFoulsExp = 1 - (totalFouls / gameTime);
            var totalFoulsValue = totalFoulsExp > 1 ? 0 : (totalFoulsExp * foulsCoef);

            //Cards
            //------
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, YELLOW_CARDS, out double homeYellowCards);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, YELLOW_CARDS, out double awayYellowCards);

            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, RED_CARDS, out double homeRedCards);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, RED_CARDS, out double awayRedCards);

            var yellowCardsTotal = homeYellowCards + awayYellowCards;
            var yellowCardsPer10Min = yellowCardsTotal / 10d;

            var homeTeamRedCards = (eventInfo.HomeTeamScore > eventInfo.AwayTeamScore && homeRedCards > awayRedCards) ? homeRedCards : 0d;
            var awayTeamRedCards = (eventInfo.AwayTeamScore > eventInfo.HomeTeamScore && awayRedCards > homeRedCards) ? awayRedCards : 0d;
            var redCards = homeTeamRedCards + awayTeamRedCards;

            var cardsValue = yellowCardsPer10Min + (2 * redCards);
            var totalCardsValue = (1 - cardsValue) * cardsCoef;

            //Ball Possession
            //----
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, BALL_POSSESSION, out double homePosession);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, BALL_POSSESSION, out double awayPosession);
            var possessionTeam = Math.Abs(homePosession - 50);
            var totalPossession = 1 - (possessionTeam / 50);
            var possessionValue = totalPossession * possessionCoef;

            //Big Chance
            //-----
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, BALL_POSSESSION, out double homeBigChance);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, BALL_POSSESSION, out double awayBigChance);
            var bigChancesTotal = (homeBigChance + awayBigChance);

            var bigChancesValue = ((bigChancesTotal / totalShots) / 10d) * bigChangesCoef;

            //TOTAl
            var liveScore = scoreLineValue + shotTargetValue + xGoalsValue
                 + totalFoulsValue + totalCardsValue + possessionValue + bigChancesValue;

            var liveMatch = new LiveMatch()
            {
                MatchId = match.Id,
                RegisteredDate = DateTime.UtcNow,
                HomeScore = eventInfo.HomeTeamScore,
                AwayScore = eventInfo.AwayTeamScore,
                Minutes = gameTime,
                ExcitmentScore = liveScore,
                ScoreLineScore = scoreLineValue / scoreLineCoef,
                ShotTargetScore = shotTargetValue / shotTargetCoef,
                XGoalsScore = xGoalsValue / xGoalsCoef,
                TotalFoulsScore = totalFoulsValue / foulsCoef,
                TotalCardsScore = totalCardsValue / cardsCoef,
                PossesionScore = possessionValue / possessionCoef,
                BigChancesScore = bigChancesValue / bigChangesCoef,
            };

            match.UpdatedDateUTC = DateTime.UtcNow;
            match.HomeScore = eventInfo.HomeTeamScore;
            match.AwayScore = eventInfo.AwayTeamScore;
            match.ExcitmentScore = eventInfo.AwayTeamScore;
            match.IsLive = true;


            return liveMatch;

        }

    }
}
