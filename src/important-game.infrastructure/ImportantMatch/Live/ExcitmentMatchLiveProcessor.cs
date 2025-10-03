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
            {
                match.MatchStatus = MatchStatus.Finished;
                return null;
            }

            var eventInfo = await leagueProcessor.GetEventInformationAsync(match.Id.ToString());
            if (eventInfo == null) return null;

            var gameTime = eventInfo.Status.GetGameTime();

            var scoreLineCoef = 0.2d;
            var xGoalsCoef = 0.2d;
            var foulsCoef = 0.15d;
            var cardsCoef = 0.15d;
            var possessionCoef = 0.15d;
            var bigChanceCoef = 0.15d;


            //Score line
            //--------
            var scoreLineDiff = Math.Abs(eventInfo.HomeTeamScore - eventInfo.AwayTeamScore);
            var scoreLineData = 1d - (scoreLineDiff / 4d);
            var scoreLineValue = scoreLineData * (1d + (gameTime / 90d));

            //Shots
            //---------
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, TOTAL_SHOTS_GOAL, out double homeTotalShots);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, TOTAL_SHOTS_GOAL, out double awayTotalShots);
            var totalShots = homeTotalShots + awayTotalShots;
            var totalShotsPerGameTime = totalShots / Math.Max(1, gameTime);
            var shotsTimeValue = Math.Min(1.0, totalShotsPerGameTime / 8d);

            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, SHOTS_ON_GOAL, out double homeShotsOnGoal);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, SHOTS_ON_GOAL, out double awayShotsOnGoal);
            var shotsOnTarget = homeShotsOnGoal + awayShotsOnGoal;
            var shotsOnTargetRatio = totalShots > 0d ? shotsOnTarget / totalShots : 0d;

            //Expected Goals
            //-----
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, XGOALS, out double xGoalsHome);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, XGOALS, out double xGoalsAway);

            var totalXGoals = xGoalsHome + xGoalsAway;
            var xGoalsValue = (totalXGoals / 3d) * (1d + (gameTime / 90d));


            //Fouls
            //------
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, FOULS, out double homeFouls);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, FOULS, out double awayFouls);
            var totalFouls = homeFouls + awayFouls;
            var totalFoulsExp = 1d - Math.Min(1.0, totalFouls / 25d);
            var totalFoulsValue = totalFoulsExp * (1d + (gameTime / 90d));

            //Cards
            //------
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, YELLOW_CARDS, out double homeYellowCards);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, YELLOW_CARDS, out double awayYellowCards);
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, RED_CARDS, out double homeRedCards);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, RED_CARDS, out double awayRedCards);

            var yellowTotal = homeYellowCards + awayYellowCards;
            var redTotal = homeRedCards + awayRedCards;
            var homeRedBoost = (eventInfo.HomeTeamScore > eventInfo.AwayTeamScore && homeRedCards > awayRedCards) ? 0.15d * homeRedCards : 0d;
            var awayRedBoost = (eventInfo.AwayTeamScore > eventInfo.HomeTeamScore && awayRedCards > homeRedCards) ? 0.15d * awayRedCards : 0d;
            var redCardBoost = (redTotal * 0.2d) + homeRedBoost + awayRedBoost;
            var cardsValue = redCardBoost + (1d - ((yellowTotal + redTotal) / 8d));
            var totalCardsValue = cardsValue * (1d + (gameTime / 90d));

            //Ball Possession
            //----
            _ = matchLiveData.GetHomeStatValue(ALL, MATCH_OVERVIEW, BALL_POSSESSION, out double homePosession);
            _ = matchLiveData.GetAwayStatValue(ALL, MATCH_OVERVIEW, BALL_POSSESSION, out double awayPosession);
            
            double losingTeamPossession;
            if (eventInfo.HomeTeamScore > eventInfo.AwayTeamScore)
            {
                losingTeamPossession = awayPosession;
            }
            else if (eventInfo.HomeTeamScore < eventInfo.AwayTeamScore)
            {
                losingTeamPossession = homePosession;
            }
            else
            {
                losingTeamPossession = Math.Min(homePosession, awayPosession);
            }

            double losingTeamBonus = 0d;
            if (losingTeamPossession > 65d)
            {
                losingTeamBonus = 0.4d;
            }
            else if (losingTeamPossession > 60d)
            {
                losingTeamBonus = 0.25d;
            }
            else if (losingTeamPossession > 55d)
            {
                losingTeamBonus = 0.15d;
            }

            var possessionDiff = Math.Abs(homePosession - awayPosession) / 200d;
            var possessionTeam = 1d - possessionDiff;
            var possessionValue = (possessionTeam * (1d + (gameTime / 90d))) + losingTeamBonus;

            //Big Chances
            //-----
            var bigChancesTotal = (shotsOnTargetRatio * totalShots) / 2d + totalXGoals;
            var bigChancesValue = (bigChancesTotal / 8d) * (1d + (gameTime / 90d));

            //Total
            var componentScores = new[]
            {
                scoreLineValue * scoreLineCoef,
                xGoalsValue * xGoalsCoef,
                totalFoulsValue * foulsCoef,
                totalCardsValue * cardsCoef,
                possessionValue * possessionCoef,
                bigChancesValue * bigChanceCoef
            };

            var weightedSum = componentScores.Sum();
            var normalizedScore = weightedSum + (shotsTimeValue * 0.1);
            var excitementDelta = ((normalizedScore * 0.4d) + 0.1d);
            var liveExcitement = match.ExcitmentScore + excitementDelta;

            var liveMatch = new LiveMatch()
            {
                MatchId = match.Id,
                RegisteredDate = DateTime.UtcNow,
                HomeScore = eventInfo.HomeTeamScore,
                AwayScore = eventInfo.AwayTeamScore,
                Minutes = gameTime,
                ExcitmentScore = Math.Min(1.0, liveExcitement),
                ScoreLineScore = scoreLineValue,
                XGoalsScore = xGoalsValue,
                TotalFoulsScore = totalFoulsValue,
                TotalCardsScore = totalCardsValue,
                PossesionScore = possessionValue,
                BigChancesScore = bigChancesValue
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

