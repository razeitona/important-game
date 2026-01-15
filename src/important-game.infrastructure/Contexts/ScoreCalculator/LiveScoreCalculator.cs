using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Providers.ExternalServices.SofaScoreAPI.Models.SofaScoreDto;
using important_game.infrastructure.Contexts.ScoreCalculator.Utils;
using Microsoft.Extensions.Logging;
using static System.Formats.Asn1.AsnWriter;

namespace important_game.infrastructure.Contexts.ScoreCalculator
{
    /// <summary>
    /// Calculates live excitement scores based on real-time match statistics from SofaScore.
    /// The live score starts from the pre-match ExcitementScore and evolves with match events.
    /// Uses contextual analysis (who's winning/losing, league positions) for nuanced scoring.
    /// </summary>
    internal class LiveScoreCalculator
    {
        private readonly ILogger<LiveScoreCalculator> _logger;

        public LiveScoreCalculator(ILogger<LiveScoreCalculator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Calculates the new LiveExcitementScore based on current live statistics and match context.
        /// </summary>
        /// <param name="baselineScore">Pre-match ExcitementScore (0-100)</param>
        /// <param name="currentLiveScore">Current LiveExcitementScore if exists, null for first calculation</param>
        /// <param name="eventData">Live event data (includes current score)</param>
        /// <param name="statistics">Live match statistics from SofaScore</param>
        /// <param name="homeTeamPosition">Home team league position (999 if unknown)</param>
        /// <param name="awayTeamPosition">Away team league position (999 if unknown)</param>
        /// <returns>Tuple of (newLiveScore, components)</returns>
        public (double newLiveScore, LiveScoreComponents components) CalculateLiveScore(
            double baselineScore,
            double? currentLiveScore,
            SSEvent eventData,
            SSEventStatistics statistics,
            int homeTeamPosition,
            int awayTeamPosition)
        {
            // Determine the baseline to use
            double baseline = currentLiveScore ?? baselineScore;

            // Extract current score
            int homeGoals = eventData.HomeScore.Current;
            int awayGoals = eventData.AwayScore.Current;

            // Calculate individual components from live statistics with context
            var components = CalculateComponents(statistics, homeGoals, awayGoals, homeTeamPosition, awayTeamPosition);

            // Calculate match elapsed time to determine progressive weighting
            int elapsedMinutes = CalculateElapsedMinutes(eventData);
            var (baseWeight, liveWeight) = CalculateProgressiveWeights(elapsedMinutes);

            // Combine baseline with live components using progressive weights
            // Early game: More weight on baseline (pre-match analysis)
            // Late game: More weight on live data (what's actually happening)
            double calculatedScore = (baseline * baseWeight) + (components.TotalLiveBonus * liveWeight);

            // Clamp to 0-100 range
            var finalScore = Math.Clamp(calculatedScore, 0d, 1d);

            _logger.LogDebug(
                "LiveScore calculation: Baseline={Baseline}, CurrentLive={CurrentLive}, " +
                "Score={HomeGoals}-{AwayGoals}, ElapsedMin={ElapsedMin}, BaseWeight={BaseWeight}, LiveWeight={LiveWeight}, " +
                "Components={Components}, Final={Final}",
                baseline, currentLiveScore, homeGoals, awayGoals, elapsedMinutes, baseWeight, liveWeight,
                components.TotalLiveBonus, finalScore);

            return (finalScore, components);
        }

        /// <summary>
        /// Calculates individual score components from live match statistics with contextual awareness.
        /// </summary>
        private LiveScoreComponents CalculateComponents(
            SSEventStatistics statistics,
            int homeGoals,
            int awayGoals,
            int homeTeamPosition,
            int awayTeamPosition)
        {
            var components = new LiveScoreComponents();

            components.ScoreLineScore = CalculateScoreLineScore(homeGoals, awayGoals, homeTeamPosition, awayTeamPosition);

            // Extract statistics from "ALL" period
            foreach (var period in statistics.Statistics)
            {
                if (period.Period != "ALL")
                    continue;

                decimal homeRedCards = 0, awayRedCards = 0;

                foreach (var group in period.Groups)
                {
                    foreach (var stat in group.StatisticsItems)
                    {
                        switch (stat.Name)
                        {
                            case "Expected goals":
                                var homeXg = ParseDouble(stat.Home);
                                var awayXg = ParseDouble(stat.Away);
                                components.XGoalsScore = CalculateXGoalsScore(homeXg, awayXg, homeGoals, awayGoals);
                                break;

                            case "Fouls":
                                components.TotalFoulsScore = CalculateFoulsScore((decimal)stat.HomeValue, (decimal)stat.AwayValue);
                                break;

                            case "Red cards":
                                homeRedCards = (decimal)stat.HomeValue;
                                awayRedCards = (decimal)stat.AwayValue;
                                break;

                            case "Yellow cards":
                                components.TotalCardsScore += CalculateYellowCardsScore((decimal)stat.HomeValue, (decimal)stat.AwayValue);
                                break;

                            case "Ball possession":
                                TryParsePercentage(stat.Home, out var homePossession);
                                TryParsePercentage(stat.Away, out var awayPossession);
                                components.PossessionScore = CalculatePossessionScore(homePossession, awayPossession, homeGoals, awayGoals);
                                break;

                            case "Big chances":
                                components.BigChancesScore = CalculateBigChancesScore(
                                    (decimal)stat.HomeValue, (decimal)stat.AwayValue,
                                    homeGoals, awayGoals);
                                break;
                        }
                    }
                }

                // Add red cards bonus/penalty
                components.TotalCardsScore += CalculateRedCardsScore(homeRedCards, awayRedCards, homeGoals, awayGoals);
                components.TotalCardsScore = Math.Min(components.TotalCardsScore, 1.0); // Cap at 1.0
                if (components.TotalCardsScore == 0)
                    components.TotalCardsScore = 100d;

                components.TotalCardsScore =  Math.Round(Math.Min(components.TotalCardsScore / 100d, 1d), 3);
            }

            // Calculate total using coefficients
            double scoreLineScore = components.ScoreLineScore * CalculatorCoeficients.ScoreLineCoef;
            double xGoalsScore = components.XGoalsScore * CalculatorCoeficients.XGoalsCoef;
            double foulsScore = components.TotalFoulsScore * CalculatorCoeficients.TotalFoulsCoef;
            double cardsScore = components.TotalCardsScore * CalculatorCoeficients.TotalCardsCoef;
            double possessionScore = components.PossessionScore * CalculatorCoeficients.PosessionCoef;
            double bigChancesScore = components.BigChancesScore * CalculatorCoeficients.BigChancesCoef;

            components.TotalLiveBonus = scoreLineScore + xGoalsScore + foulsScore + cardsScore +
                                         possessionScore + bigChancesScore;
            components.TotalLiveBonus = Math.Round(components.TotalLiveBonus, 3);

            return components;
        }

        /// <summary>
        /// Calculates ScoreLineScore based on goals, competitiveness, and underdog factor.
        /// Positive impact: Goals + competitiveness + underdog winning.
        /// Range: 0-100 (before division by 100)
        /// </summary>
        private double CalculateScoreLineScore(decimal homeGoals, decimal awayGoals, int homePosition, int awayPosition)
        {
            int totalGoals = (int)(homeGoals + awayGoals);
            int goalDifference = Math.Abs((int)(homeGoals - awayGoals));
            double score = 0d;

            // Base points for total goals (more goals = more excitement)
            score += totalGoals switch
            {
                1 or 2 => 30,
                3 or 4 => 60,
                >= 5 => 80,
                _ => 0
            };

            // Competitiveness bonus (close games)
            score -= goalDifference switch
            {
                0 => 0,  // Draw
                1 => 10,  // Very competitive
                2 => 20,  // Competitive
                _ => 30
            };

   

            // Underdog bonus: Lower positioned team (higher number) winning
            if (homePosition < 999 && awayPosition < 999) // Only if we have standings data
            {
                if (homeGoals > awayGoals && homePosition > awayPosition)
                {
                    // Home (underdog) winning
                    int positionDiff = homePosition - awayPosition;
                    score += Math.Min(positionDiff * 2, 40); // Up to +40 for big upset
                }
                else if (awayGoals > homeGoals && awayPosition > homePosition)
                {
                    // Away (underdog) winning
                    int positionDiff = awayPosition - homePosition;
                    score += Math.Min(positionDiff * 2, 40); // Up to +40 for big upset
                }
            }

            score =  Math.Clamp(score / 100d, 0d, 1d);
            return score;
        }

        /// <summary>
        /// Calculates XGoalsScore with contextual awareness.
        /// Positive impact: High total xG, balanced xG, losing team with more xG.
        /// Range: 0-100 (before division by 100)
        /// </summary>
        private double CalculateXGoalsScore(decimal homeXg, decimal awayXg, int homeGoals, int awayGoals)
        {
            double totalXg = (double)(homeXg + awayXg);
            double score = 0d;

            // Base score for total xG (want goals)
            score += totalXg switch
            {
                < 1.0 => 0,
                < 2.0 => 30,
                < 3.0 => 60,
                >= 3.0 => 80,
                _ => 0
            };

            // Bonus for balanced xG (competitive chances)
            double xgDiff = Math.Abs((double)(homeXg - awayXg));
            if (xgDiff < 0.5) score += 20;        // Very balanced
            else if (xgDiff < 1.0) score += 10;   // Balanced

            // Bonus if losing team has more xG (recovery potential = excitement)
            if (homeGoals < awayGoals && homeXg > awayXg && (homeXg - awayXg) > 0.3m)
            {
                // Home losing but creating more chances
                score += 30;
            }
            else if (awayGoals < homeGoals && awayXg > homeXg && (awayXg - homeXg) > 0.3m)
            {
                // Away losing but creating more chances
                score += 30;
            }

            return Math.Round(Math.Min(score / 100d, 1d), 3);
        }

        /// <summary>
        /// Calculates TotalFoulsScore (NEGATIVE impact).
        /// Too many fouls = messy game = less exciting.
        /// Range: 0-100 (penalty, before division by 100)
        /// </summary>
        private double CalculateFoulsScore(decimal homeFouls, decimal awayFouls)
        {
            int totalFouls = (int)(homeFouls + awayFouls);

            // Penalty scale (negative impact)
            double penalty = totalFouls switch
            {
                <= 15 => 0,      // Clean game, no penalty
                <= 20 => -20,    // Moderate fouls, small penalty
                <= 25 => -40,    // Many fouls, medium penalty
                _ => -60         // Too many fouls, heavy penalty
            };

            // Convert penalty to 0-100 scale (inverted - lower is better)
            // -60 penalty → 40 score, 0 penalty → 100 score
            var score =  Math.Clamp(100 + penalty, 0, 100);

            return Math.Round(Math.Min(score / 100d, 1d), 3);
        }

        /// <summary>
        /// Calculates yellow cards score (NEGATIVE base impact).
        /// Many yellow cards = aggressive/messy game.
        /// Range: 0-100 (penalty, before division by 100)
        /// </summary>
        private double CalculateYellowCardsScore(decimal homeYellow, decimal awayYellow)
        {
            int totalYellow = (int)(homeYellow + awayYellow);

            // Penalty for excessive yellows
            double penalty = totalYellow switch
            {
                <= 2 => 0,       // Normal, no penalty
                <= 4 => -15,     // Few yellows, small penalty
                <= 6 => -30,     // Many yellows, medium penalty
                _ => -50         // Too many yellows, heavy penalty
            };

            return penalty;
        }

        /// <summary>
        /// Calculates red cards score (contextual).
        /// Base: NEGATIVE (messy game)
        /// Bonus: Winning team getting red card = comeback potential = POSITIVE
        /// </summary>
        private double CalculateRedCardsScore(decimal homeRed, decimal awayRed, int homeGoals, int awayGoals)
        {
            int totalRed = (int)(homeRed + awayRed);
            double score = 0d;

            // Base penalty for red cards
            score += totalRed * -40; // Each red = -40

            // Contextual bonus: Winning team with red card = potential comeback
            if (homeGoals > awayGoals && homeRed > 0)
            {
                // Home winning but got red card(s)
                score += (double)homeRed * 60; // Converts to positive (+20 net per red)
            }
            else if (awayGoals > homeGoals && awayRed > 0)
            {
                // Away winning but got red card(s)
                score += (double)awayRed * 60; // Converts to positive (+20 net per red)
            }

            return score;
        }

        /// <summary>
        /// Calculates PossessionScore with contextual awareness.
        /// Positive impact: Balanced possession, losing team with more possession.
        /// Range: 0-100 (before division by 100)
        /// </summary>
        private double CalculatePossessionScore(decimal homePossession, decimal awayPossession, int homeGoals, int awayGoals)
        {
            decimal difference = Math.Abs(homePossession - awayPossession);
            double score = 0d;

            // Base score for balanced possession (competitive)
            score += difference switch
            {
                <= 10 => 70,  // Very balanced
                <= 20 => 50,  // Balanced
                <= 30 => 30,  // Somewhat balanced
                _ => 10       // Unbalanced
            };

            // Bonus if losing team has more possession (dominating without converting)
            if (homeGoals < awayGoals && homePossession > awayPossession && (homePossession - awayPossession) > 5)
            {
                // Home losing but dominating possession
                score += 30;
            }
            else if (awayGoals < homeGoals && awayPossession > homePossession && (awayPossession - homePossession) > 5)
            {
                // Away losing but dominating possession
                score += 30;
            }

            return Math.Round(Math.Min(score / 100d, 1d), 3);
        }

        /// <summary>
        /// Calculates BigChancesScore with contextual awareness.
        /// Positive impact: More chances, balanced chances, losing team creating chances.
        /// Range: 0-100 (before division by 100)
        /// </summary>
        private double CalculateBigChancesScore(decimal homeChances, decimal awayChances, int homeGoals, int awayGoals)
        {
            int totalChances = (int)(homeChances + awayChances);
            double score = 0d;

            // Base score for total chances
            score += totalChances switch
            {
                <= 2 => 20,
                <= 4 => 50,
                <= 6 => 70,
                _ => 90
            };

            // Bonus for balanced chances
            int chanceDiff = Math.Abs((int)(homeChances - awayChances));
            if (chanceDiff <= 1) score += 20;      // Very balanced
            else if (chanceDiff == 2) score += 10; // Balanced

            // Bonus if losing team creating more chances (pressure for comeback)
            if (homeGoals < awayGoals && homeChances > awayChances)
            {
                score += 25;
            }
            else if (awayGoals < homeGoals && awayChances > homeChances)
            {
                score += 25;
            }

            return Math.Round(Math.Min(score / 100d, 1d), 3) ;
        }

        /// <summary>
        /// Helper to parse double from string.
        /// </summary>
        private decimal ParseDouble(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            return decimal.TryParse(value, out decimal result) ? result : 0;
        }

        /// <summary>
        /// Helper to parse percentage strings (e.g., "52%" -> 52.0)
        /// </summary>
        private bool TryParsePercentage(string? percentageString, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(percentageString))
                return false;

            string cleaned = percentageString.Replace("%", "").Trim();
            return decimal.TryParse(cleaned, out value);
        }

        /// <summary>
        /// Calculates elapsed match time in minutes.
        /// Uses CurrentPeriodStartTimestamp and current time to estimate elapsed time.
        /// Also considers StatusTime.Initial for more accurate time tracking.
        /// </summary>
        private int CalculateElapsedMinutes(SSEvent eventData)
        {
            // Try to use StatusTime.Initial if available (more accurate)
            if (eventData.StatusTime?.Initial.HasValue == true)
            {
                int initial = eventData.StatusTime.Initial.Value;

                // Add injury time if in second half
                if (eventData.Time?.InjuryTime2.HasValue == true)
                {
                    return initial + eventData.Time.InjuryTime2.Value;
                }

                return initial;
            }

            // Fallback: Calculate from CurrentPeriodStartTimestamp
            if (eventData.CurrentPeriodStartTimestamp.HasValue)
            {
                long currentTimestampSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long periodStartSeconds = eventData.CurrentPeriodStartTimestamp.Value;
                long elapsedSeconds = currentTimestampSeconds - periodStartSeconds;
                int elapsedMinutes = (int)(elapsedSeconds / 60);

                // If in second half, add 45 minutes
                if (eventData.Status?.Description?.Contains("2nd half") == true)
                {
                    elapsedMinutes += 45;
                }

                return Math.Clamp(elapsedMinutes, 0, 120); // Max 120 minutes (90 + extra time)
            }

            // Ultimate fallback: Assume mid-game (45 minutes)
            _logger.LogWarning("Could not determine elapsed time for match, assuming 45 minutes");
            return 45;
        }

        /// <summary>
        /// Calculates progressive weights for baseline vs live score based on elapsed time.
        ///
        /// Time-based weight distribution:
        /// - 0-30 min (Early):  80% baseline, 20% live  (pre-match analysis more reliable)
        /// - 30-60 min (Mid):   50% baseline, 50% live  (balanced)
        /// - 60-90+ min (Late): 20% baseline, 80% live  (live data more important)
        ///
        /// Rationale:
        /// - Early: Limited live data, pre-match predictions still relevant
        /// - Mid: Equal weight as patterns emerge
        /// - Late: Live events dominate, outcome becoming clear
        /// </summary>
        /// <param name="elapsedMinutes">Match elapsed time in minutes (0-120)</param>
        /// <returns>Tuple of (baselineWeight, liveWeight) that sum to 1.0</returns>
        private (double baseWeight, double liveWeight) CalculateProgressiveWeights(int elapsedMinutes)
        {
            // Clamp to valid range
            int minutes = Math.Clamp(elapsedMinutes, 0, 120);

            double baseWeight;
            double liveWeight;

            if (minutes <= 30)
            {
                // Early game (0-30 min): Linear interpolation from 80% to 50% baseline
                // At 0 min: 80% baseline, 20% live
                // At 30 min: 50% baseline, 50% live
                double progress = minutes / 30.0; // 0.0 to 1.0
                baseWeight = 0.80 - (progress * 0.30); // 0.80 -> 0.50
                liveWeight = 1.0 - baseWeight;
            }
            else if (minutes <= 60)
            {
                // Mid game (30-60 min): Linear interpolation from 50% to 20% baseline
                // At 30 min: 50% baseline, 50% live
                // At 60 min: 20% baseline, 80% live
                double progress = (minutes - 30) / 30.0; // 0.0 to 1.0
                baseWeight = 0.50 - (progress * 0.30); // 0.50 -> 0.20
                liveWeight = 1.0 - baseWeight;
            }
            else
            {
                // Late game (60-120 min): Stay at 20% baseline, 80% live
                // Outcome is being determined by live events
                baseWeight = 0.20;
                liveWeight = 0.80;
            }

            _logger.LogDebug(
                "Progressive weights: ElapsedMin={ElapsedMin}, BaseWeight={BaseWeight:P0}, LiveWeight={LiveWeight:P0}",
                minutes, baseWeight, liveWeight);

            return (baseWeight, liveWeight);
        }
    }
}
