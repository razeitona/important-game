using important_game.infrastructure.ImportantMatch.Data.Entities;
using important_game.infrastructure.ImportantMatch.Models.Processors;
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

            var scoreLineCoef = 0.2d;
            //var shotTargetCoef = 0.2d;
            var xGoalsCoef = 0.2d;
            var foulsCoef = 0.15d;
            var cardsCoef = 0.15d;
            var possessionCoef = 0.15d;
            var bigChangesCoef = 0.15d;


            //Score line
            //--------
            var scoreGameDif = Math.Abs(eventInfo.HomeTeamScore - eventInfo.AwayTeamScore);
            var scoreLineData = 1d - (scoreGameDif / 4d);
            var scoreLineValue = scoreLineData * (1d + (gameTime / 100d));

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

            //var shotTargetValue = (totalShotsPer10Max + shotsOnTargetAverage) * shotTargetCoef;

            //Expected Goals
            //-----
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, XGOALS, out double xGoalsHome);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, XGOALS, out double xGoalsAway);

            var totalxGoals = xGoalsHome + xGoalsAway;
            var xGoalsValue = (totalxGoals / 5d) * (1d + (gameTime / 90d));


            //Fouls
            //------
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, FOULS, out double homeFouls);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, FOULS, out double awayFouls);
            var totalFouls = homeFouls + awayFouls;
            var totalFoulsExp = 1 - (totalFouls / 30d);
            var totalFoulsValue = totalFoulsExp * (1d + (gameTime / 90d));

            //Cards
            //------
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, YELLOW_CARDS, out double homeYellowCards);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, YELLOW_CARDS, out double awayYellowCards);

            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, RED_CARDS, out double homeRedCards);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, RED_CARDS, out double awayRedCards);

            var yellowCardsTotal = homeYellowCards + awayYellowCards;
            //var yellowCardsPer10Min = yellowCardsTotal / 10d;

            var homeTeamRedCards = (eventInfo.HomeTeamScore > eventInfo.AwayTeamScore && homeRedCards > awayRedCards) ? 0.10d * homeRedCards : 0d;
            var awayTeamRedCards = (eventInfo.AwayTeamScore > eventInfo.HomeTeamScore && awayRedCards > homeRedCards) ? 0.10d * awayRedCards : 0d;
            var redCards = homeRedCards + awayRedCards;

            var redCardBoost = (redCards * 0.15d) + homeTeamRedCards + awayTeamRedCards;

            var cardsValue = redCardBoost + (1d - ((yellowCardsTotal + redCards) / 10d));
            var totalCardsValue = cardsValue * (1d + (gameTime / 90d));

            //Ball Possession
            //----
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, BALL_POSSESSION, out double homePosession);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, BALL_POSSESSION, out double awayPosession);
            //var totalPossession = pos1 - (possessionTeam / 50);
            var losingTeamPossession = 0d;
            if (eventInfo.HomeTeamScore > eventInfo.AwayTeamScore)
            {
                losingTeamPossession = awayPosession;
            }
            else if (eventInfo.HomeTeamScore < eventInfo.AwayTeamScore)
            {
                losingTeamPossession = homePosession;
            }

            var losingTeamPossessionBonus = 0d;
            if (losingTeamPossession > 65d)
            {
                losingTeamPossessionBonus = 0.3d;
            }
            else if (losingTeamPossession > 55d && losingTeamPossession <= 60d)
            {
                losingTeamPossessionBonus = 0.1d;
            }
            else if (losingTeamPossession > 60d && losingTeamPossession <= 65d)
            {
                losingTeamPossessionBonus = 0.2d;
            }

            var possessionTeam = 1d - Math.Abs((homePosession - 50d) / 100d);
            var possessionValue = possessionTeam * (1d + (gameTime / 90d)) + losingTeamPossessionBonus;

            //Big Chance
            //-----
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, BALL_POSSESSION, out double homeBigChance);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, BALL_POSSESSION, out double awayBigChance);
            var bigChancesTotal = (homeBigChance + awayBigChance);

            var bigChancesValue = (bigChancesTotal / 10d) * (1d + (gameTime / 90d));

            //TOTAl
            var liveScore = scoreLineValue + xGoalsValue
                 + totalFoulsValue + totalCardsValue + possessionValue + bigChancesValue;

            liveScore = (0.5d * liveScore) - 0.3d;
            var liveExcitmenetScore = match.ExcitmentScore + liveScore;

            var liveMatch = new LiveMatch()
            {
                MatchId = match.Id,
                RegisteredDate = DateTime.UtcNow,
                HomeScore = eventInfo.HomeTeamScore,
                AwayScore = eventInfo.AwayTeamScore,
                Minutes = gameTime,
                ExcitmentScore = liveExcitmenetScore,
                ScoreLineScore = scoreLineValue / scoreLineCoef,
                //ShotTargetScore = shotTargetValue / shotTargetCoef,
                XGoalsScore = xGoalsValue / xGoalsCoef,
                TotalFoulsScore = totalFoulsValue / foulsCoef,
                TotalCardsScore = totalCardsValue / cardsCoef,
                PossesionScore = possessionValue / possessionCoef,
                BigChancesScore = bigChancesValue / bigChangesCoef,
            };

            match.UpdatedDateUTC = DateTime.UtcNow;
            match.HomeScore = eventInfo.HomeTeamScore;
            match.AwayScore = eventInfo.AwayTeamScore;

            if (eventInfo.Status.StatusCode == EventMatchStatus.Finished)
            {
                match.MatchStatus = MatchStatus.Finished;
            }
            else if (eventInfo.Status.StatusCode == EventMatchStatus.NotStarted)
            {
                match.MatchStatus = MatchStatus.Upcoming;
            }
            else if (eventInfo.Status.StatusCode == EventMatchStatus.FirstHalf || eventInfo.Status.StatusCode == EventMatchStatus.SecondHalf)
            {
                match.MatchStatus = MatchStatus.Live;
            }

            return liveMatch;

        }
    }
}
