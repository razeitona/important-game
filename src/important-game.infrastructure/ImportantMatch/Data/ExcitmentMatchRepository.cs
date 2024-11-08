using important_game.infrastructure.ImportantMatch.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace important_game.infrastructure.ImportantMatch.Data
{
    public class ExcitmentMatchRepository : IExctimentMatchRepository
    {
        #region Competition Methods

        public async Task SaveCompetitionAsync(Competition competition)
        {
            using (var context = new ImportantMatchDbContext())
            {
                // Disable change tracking for performance on bulk operations
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var existingRivalry = context.Competitions.FirstOrDefault(c => c.Id == competition.Id);

                if (existingRivalry != null)
                {
                    // Update properties of the existing competition
                    existingRivalry.Name = competition.Name;
                    existingRivalry.TitleHolderTeamId = competition.TitleHolderTeamId;
                    context.Competitions.Update(existingRivalry);
                }
                else
                {
                    // Add new competition to the list for insertion
                    context.Competitions.Add(competition);
                }

                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateCompetitionAsync(Competition competition)
        {
            using (var context = new ImportantMatchDbContext())
            {
                var existCompetition = context.Competitions.FirstOrDefault(c => c.Id == competition.Id);
                if (existCompetition != null)
                {
                    existCompetition.TitleHolderTeamId = competition.TitleHolderTeamId;
                    context.Competitions.Update(existCompetition);
                    await context.SaveChangesAsync();
                }
            }
        }

        public Competition? GetCompetitionById(int id)
        {
            using (var context = new ImportantMatchDbContext())
            {
                return context.Competitions.FirstOrDefault(c => c.Id == id);
            }
        }

        public List<Competition> GetCompetitions()
        {
            using (var context = new ImportantMatchDbContext())
            {
                return context.Competitions.ToList();
            }
        }

        public List<Competition> GetActiveCompetitions()
        {
            using (var context = new ImportantMatchDbContext())
            {
                return context.Competitions.Where(c => c.IsActive).ToList();
            }
        }

        #endregion

        #region Team Methods

        public async Task<Team> SaveTeamAsync(Team team)
        {
            using (var context = new ImportantMatchDbContext())
            {
                if (context.Teams.Where(c => c.Id == team.Id).Count() > 0)
                    return team;

                context.Teams.Add(team);
                await context.SaveChangesAsync();

                return team;
            }
        }

        public async Task<Team?> GetTeamById(int id)
        {
            using (var context = new ImportantMatchDbContext())
            {
                return context.Teams.FirstOrDefault(c => c.Id == id);
            }
        }

        public async Task<List<Team>> GetTeamsByIds(List<int> ids)
        {
            using (var context = new ImportantMatchDbContext())
            {
                return context.Teams.Where(c => ids.Contains(c.Id)).ToList();
            }
        }

        #endregion

        #region Matches Methods

        public async Task SaveMatchAsync(Match match)
        {
            using (var context = new ImportantMatchDbContext())
            {
                // Disable change tracking for performance on bulk operations
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var existingMatch = context.Matches.FirstOrDefault(c => c.Id == match.Id);

                if (existingMatch != null)
                {
                    // Update properties of the existing fixture
                    existingMatch.ExcitmentScore = match.ExcitmentScore;
                    existingMatch.CompetitionScore = match.CompetitionScore;
                    existingMatch.FixtureScore = match.FixtureScore;
                    existingMatch.FormScore = match.FormScore;
                    existingMatch.GoalsScore = match.GoalsScore;
                    existingMatch.CompetitionStandingScore = match.CompetitionStandingScore;
                    existingMatch.HeadToHeadScore = match.HeadToHeadScore;
                    existingMatch.RivalryScore = match.RivalryScore;
                    existingMatch.TitleHolderScore = match.TitleHolderScore;
                    existingMatch.isFinished = match.isFinished;
                    existingMatch.IsLive = match.IsLive;
                    existingMatch.AwayScore = match.AwayScore;
                    existingMatch.HomeScore = match.HomeScore;

                    context.Matches.Update(existingMatch);
                }
                else
                {
                    // Add new fixture to the list for insertion
                    context.Matches.Add(match);
                }

                await context.SaveChangesAsync();
            }
        }

        public async Task SaveMatchesAsync(List<Match> matches)
        {
            using (var context = new ImportantMatchDbContext())
            {
                // Disable change tracking for performance on bulk operations
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                // Get all existing fixture IDs from the database in a single query
                var matchesIds = matches.Select(f => f.Id).ToList();
                var existingMatches = context.Matches
                    .Where(f => matchesIds.Contains(f.Id))
                    .ToDictionary(f => f.Id, f => f);

                var matchesToAdd = new List<Match>();
                var matchesToUpdate = new List<Match>();

                foreach (var match in matches)
                {
                    if (existingMatches.TryGetValue(match.Id, out var existingMatch))
                    {
                        // Update properties of the existing fixture
                        existingMatch.ExcitmentScore = match.ExcitmentScore;
                        existingMatch.CompetitionScore = match.CompetitionScore;
                        existingMatch.FixtureScore = match.FixtureScore;
                        existingMatch.FormScore = match.FormScore;
                        existingMatch.GoalsScore = match.GoalsScore;
                        existingMatch.CompetitionStandingScore = match.CompetitionStandingScore;
                        existingMatch.HeadToHeadScore = match.HeadToHeadScore;
                        existingMatch.RivalryScore = match.RivalryScore;
                        existingMatch.TitleHolderScore = match.TitleHolderScore;
                        existingMatch.isFinished = match.isFinished;
                        existingMatch.IsLive = match.IsLive;
                        existingMatch.AwayScore = match.AwayScore;
                        existingMatch.HomeScore = match.HomeScore;

                        matchesToUpdate.Add(existingMatch);
                    }
                    else
                    {
                        // Add new fixture to the list for insertion
                        matchesToAdd.Add(match);
                    }
                }

                // Perform bulk insert for new fixtures
                if (matchesToAdd.Any())
                {
                    context.Matches.AddRange(matchesToAdd);
                }

                // Perform bulk update for existing fixtures
                if (matchesToUpdate.Any())
                {
                    context.Matches.UpdateRange(matchesToUpdate);
                }

                // Save changes in a single transaction for efficiency
                await context.SaveChangesAsync();

                // Re-enable tracking
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            }
        }

        public Match? GetMatchById(int id)
        {
            using (var context = new ImportantMatchDbContext())
            {
                return context.Matches
                        .Include(c => c.LiveMatches)
                        .Include(c => c.Competition)
                        .Include(c => c.HomeTeam)
                        .Include(c => c.AwayTeam)
                        .Include(c => c.HeadToHead)
                        .FirstOrDefault(c => c.Id == id);
            }
        }

        public async Task<List<Match>> GetUpcomingMatchesAsync()
        {
            using (var context = new ImportantMatchDbContext())
            {
                return await context.Matches
                        .Include(c => c.Competition)
                        .Include(c => c.HomeTeam)
                        .Include(c => c.AwayTeam)
                        .Where(c => !c.isFinished && !c.IsLive)
                        .ToListAsync();
            }
        }

        public List<Match> GetMatchesFromCompetition(int competitionId)
        {
            using (var context = new ImportantMatchDbContext())
            {
                return context.Matches.Where(c => c.CompetitionId == competitionId).ToList();
            }
        }




        public List<Match> GetCompetitionActiveMatches(int competitionId)
        {
            using (var context = new ImportantMatchDbContext())
            {
                return context.Matches.Where(c => c.CompetitionId == competitionId && !c.isFinished).ToList();
            }
        }


        public List<Match> GetUpcomingMatchesFromCompetition(int competitionId)
        {
            using (var context = new ImportantMatchDbContext())
            {
                return context.Matches.Where(c => c.CompetitionId == competitionId && !c.isFinished && !c.IsLive).ToList();
            }
        }

        public List<Match> GetLiveMatchesFromCompetition(int competitionId)
        {
            using (var context = new ImportantMatchDbContext())
            {
                return context.Matches.Where(c => c.CompetitionId == competitionId && !c.isFinished && c.IsLive).ToList();
            }
        }

        public List<Match> GetLiveMatches()
        {
            using (var context = new ImportantMatchDbContext())
            {
                return context.Matches
                    .Include(c => c.LiveMatches)
                    .Include(c => c.Competition)
                    .Include(c => c.HomeTeam)
                    .Include(c => c.AwayTeam)
                    .Where(c => !c.isFinished && c.IsLive).ToList();
            }
        }

        public List<Match> GetFinishedMatchesFromCompetition(int competitionId)
        {
            using (var context = new ImportantMatchDbContext())
            {
                return context.Matches.Where(c => c.CompetitionId == competitionId && c.isFinished).ToList();
            }
        }


        public async Task SaveLiveMatchAsync(LiveMatch liveMatch)
        {
            using (var context = new ImportantMatchDbContext())
            {
                // Disable change tracking for performance on bulk operations
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var existingLiveMatch = context.LiveMatches.FirstOrDefault(c => c.Id == liveMatch.Id);

                if (existingLiveMatch != null)
                {
                    // Update properties of the existing fixture
                    existingLiveMatch.ExcitmentScore = liveMatch.ExcitmentScore;
                    existingLiveMatch.ScoreLineScore = liveMatch.ScoreLineScore;
                    existingLiveMatch.ShotTargetScore = liveMatch.ShotTargetScore;
                    existingLiveMatch.XGoalsScore = liveMatch.XGoalsScore;
                    existingLiveMatch.TotalFoulsScore = liveMatch.TotalFoulsScore;
                    existingLiveMatch.TotalCardsScore = liveMatch.TotalCardsScore;
                    existingLiveMatch.PossesionScore = liveMatch.PossesionScore;
                    existingLiveMatch.BigChancesScore = liveMatch.BigChancesScore;

                    context.LiveMatches.Update(existingLiveMatch);
                }
                else
                {
                    // Add new fixture to the list for insertion
                    context.LiveMatches.Add(liveMatch);
                }

                await context.SaveChangesAsync();
            }
        }

        public async Task SaveLiveMatchesAsync(List<LiveMatch> liveMatches)
        {
            using (var context = new ImportantMatchDbContext())
            {
                // Disable change tracking for performance on bulk operations
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                // Get all existing fixture IDs from the database in a single query
                var liveMatchesIds = liveMatches.Select(f => f.Id).ToList();
                var existingLiveMatches = context.LiveMatches
                    .Where(f => liveMatchesIds.Contains(f.Id))
                    .ToDictionary(f => f.Id, f => f);

                var liveMatchesToAdd = new List<LiveMatch>();
                var liveMatchesToUpdate = new List<LiveMatch>();

                foreach (var liveMatch in liveMatches)
                {
                    if (existingLiveMatches.TryGetValue(liveMatch.Id, out var existingMatch))
                    {
                        // Update properties of the existing fixture
                        existingMatch.ExcitmentScore = liveMatch.ExcitmentScore;
                        existingMatch.ScoreLineScore = liveMatch.ScoreLineScore;
                        existingMatch.ShotTargetScore = liveMatch.ShotTargetScore;
                        existingMatch.XGoalsScore = liveMatch.XGoalsScore;
                        existingMatch.TotalFoulsScore = liveMatch.TotalFoulsScore;
                        existingMatch.TotalCardsScore = liveMatch.TotalCardsScore;
                        existingMatch.PossesionScore = liveMatch.PossesionScore;
                        existingMatch.BigChancesScore = liveMatch.BigChancesScore;

                        liveMatchesToUpdate.Add(existingMatch);
                    }
                    else
                    {
                        // Add new fixture to the list for insertion
                        liveMatchesToAdd.Add(liveMatch);
                    }
                }

                // Perform bulk insert for new fixtures
                if (liveMatchesToAdd.Any())
                {
                    context.LiveMatches.AddRange(liveMatchesToAdd);
                }

                // Perform bulk update for existing fixtures
                if (liveMatchesToUpdate.Any())
                {
                    context.LiveMatches.UpdateRange(liveMatchesToUpdate);
                }

                // Save changes in a single transaction for efficiency
                await context.SaveChangesAsync();

                // Re-enable tracking
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            }
        }

        public async Task<LiveMatch?> GetLiveMatchById(int id)
        {
            using (var context = new ImportantMatchDbContext())
            {
                return context.LiveMatches.FirstOrDefault(c => c.Id == id);
            }
        }


        public async Task SaveHeadToHeadMatchesAsync(List<Headtohead> headtoheadMatches)
        {
            using (var context = new ImportantMatchDbContext())
            {
                // Disable change tracking for performance on bulk operations
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                if (headtoheadMatches.Count == 0)
                    return;

                var matchId = headtoheadMatches!.FirstOrDefault()!.MatchId;

                // Get all existing fixture IDs from the database in a single query
                var existingHeadToHeadMatches = context.HeadtoheadMatches
                    .Where(f => f.MatchId == matchId);

                context.HeadtoheadMatches.RemoveRange(existingHeadToHeadMatches);

                context.HeadtoheadMatches.AddRange(headtoheadMatches);

                // Save changes in a single transaction for efficiency
                await context.SaveChangesAsync();

                // Re-enable tracking
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            }
        }

        #endregion


        #region Rivalry Methods


        public async Task SaveRivalryAsync(Rivalry rivalry)
        {
            using (var context = new ImportantMatchDbContext())
            {
                // Disable change tracking for performance on bulk operations
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var existingRivalry = context.Rivalries.FirstOrDefault(c => c.Id == rivalry.Id);

                if (existingRivalry != null)
                {
                    // Update properties of the existing rivalry
                    existingRivalry.RivarlyValue = rivalry.RivarlyValue;
                    context.Rivalries.Update(existingRivalry);
                }
                else
                {
                    // Add new rivalry to the list for insertion
                    context.Rivalries.Add(rivalry);
                }

                await context.SaveChangesAsync();
            }
        }

        public Rivalry? GetRivalryByTeamId(int teamOneId, int teamTwoId)
        {
            using (var context = new ImportantMatchDbContext())
            {
                return context.Rivalries.Where(c =>
                            (c.TeamOneId == teamOneId && c.TeamTwoId == teamTwoId)
                            || (c.TeamTwoId == teamOneId && c.TeamOneId == teamTwoId)
                            )
                    .FirstOrDefault();
            }
        }


        #endregion
    }
}
