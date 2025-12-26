using System.Diagnostics.CodeAnalysis;
using important_game.infrastructure.ImportantMatch.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace important_game.infrastructure.ImportantMatch.Data
{
    [ExcludeFromCodeCoverage]
    public class ExcitmentMatchRepository : IExctimentMatchRepository
    {
        #region Competition Methods

        private readonly ImportantMatchDbContext _context;

        public ExcitmentMatchRepository(ImportantMatchDbContext context)
        {
            _context = context;
        }

        public async Task SaveCompetitionAsync(Competition competition)
        {
            var previousTracking = _context.ChangeTracker.QueryTrackingBehavior;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            try
            {
                var existingCompetition = await _context.Competitions.FirstOrDefaultAsync(c => c.Id == competition.Id);

                if (existingCompetition != null)
                {
                    existingCompetition.Name = competition.Name;
                    existingCompetition.Code = competition.Code;
                    existingCompetition.TitleHolderTeamId = competition.TitleHolderTeamId;
                    _context.Competitions.Update(existingCompetition);
                }
                else
                {
                    _context.Competitions.Add(competition);
                }

                await _context.SaveChangesAsync();
            }
            finally
            {
                _context.ChangeTracker.QueryTrackingBehavior = previousTracking;
            }
        }

        public async Task UpdateCompetitionAsync(Competition competition)
        {
            var existCompetition = await _context.Competitions.FirstOrDefaultAsync(c => c.Id == competition.Id);
            if (existCompetition != null)
            {
                existCompetition.TitleHolderTeamId = competition.TitleHolderTeamId;
                if (!string.IsNullOrWhiteSpace(competition.Code))
                {
                    existCompetition.Code = competition.Code;
                }
                _context.Competitions.Update(existCompetition);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Competition?> GetCompetitionByIdAsync(int id)
        {
            return await _context.Competitions.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Competition>> GetCompetitionsAsync()
        {
            return await _context.Competitions.ToListAsync();
        }

        public async Task<List<Competition>> GetActiveCompetitionsAsync()
        {
            return await _context.Competitions.Where(c => c.IsActive).ToListAsync();
        }

        #endregion

        #region Team Methods

        public async Task<Team> SaveTeamAsync(Team team)
        {
            var teamExists = await _context.Teams
                .AsNoTracking()
                .AnyAsync(c => c.Id == team.Id);

            if (teamExists)
            {
                return team;
            }

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return team;
        }

        public async Task<Team?> GetTeamByIdAsync(int id)
        {
            return await _context.Teams.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Team>> GetTeamsByIdsAsync(List<int> ids)
        {
            return await _context.Teams.Where(c => ids.Contains(c.Id)).ToListAsync();
        }

        #endregion

        #region Matches Methods

        public async Task SaveMatchAsync(Match match)
        {
            // Disable change tracking for performance on bulk operations
            //_context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var existingMatch = await _context.Matches.FirstOrDefaultAsync(c => c.Id == match.Id);

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
                existingMatch.MatchStatus = match.MatchStatus;
                existingMatch.AwayScore = match.AwayScore;
                existingMatch.HomeScore = match.HomeScore;

                _context.Matches.Update(existingMatch);
            }
            else
            {
                // Add new fixture to the list for insertion
                _context.Matches.Add(match);
            }

            await _context.SaveChangesAsync();
        }

        public async Task SaveMatchesAsync(List<Match> matches)
        {
            // Disable change tracking for performance on bulk operations
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Get all existing fixture IDs from the database in a single query
            var matchesIds = matches.Select(f => f.Id).ToList();
            var existingMatches = await _context.Matches
                .Where(f => matchesIds.Contains(f.Id))
                .ToDictionaryAsync(f => f.Id, f => f);

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
                    existingMatch.MatchStatus = match.MatchStatus;
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
                _context.Matches.AddRange(matchesToAdd);
            }

            // Perform bulk update for existing fixtures
            if (matchesToUpdate.Any())
            {
                _context.Matches.UpdateRange(matchesToUpdate);
            }

            // Save changes in a single transaction for efficiency
            await _context.SaveChangesAsync();

            // Re-enable tracking
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        }

        public async Task<Match?> GetMatchByIdAsync(int id)
        {
            return await _context.Matches
                    .Include(c => c.LiveMatches)
                    .Include(c => c.Competition)
                    .Include(c => c.HomeTeam)
                    .Include(c => c.AwayTeam)
                    .Include(c => c.HeadToHead)
                    .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Match>> GetUpcomingMatchesAsync()
        {
            return await _context.Matches
                    .Include(c => c.Competition)
                    .Include(c => c.HomeTeam)
                    .Include(c => c.AwayTeam)
                    .Include(c => c.LiveMatches)
                    .Where(c => c.MatchStatus != MatchStatus.Finished)
                    .ToListAsync();
        }

        public async Task<List<Match>> GetMatchesFromCompetitionAsync(int competitionId)
        {
            return await _context.Matches.Where(c => c.CompetitionId == competitionId).ToListAsync();
        }


        public async Task<List<Match>> GetCompetitionActiveMatchesAsync(int competitionId)
        {
            return await _context.Matches
                .Where(c => c.CompetitionId == competitionId && c.MatchStatus != MatchStatus.Finished)
                .ToListAsync();
        }

        public async Task<List<Match>> GetUpcomingMatchesFromCompetitionAsync(int competitionId)
        {
            return await _context.Matches
                .Where(c => c.CompetitionId == competitionId && c.MatchStatus == MatchStatus.Upcoming)
                .ToListAsync();
        }

        public async Task<List<Match>> GetLiveMatchesFromCompetitionAsync(int competitionId)
        {
            return await _context.Matches
                .Where(c => c.CompetitionId == competitionId && c.MatchStatus == MatchStatus.Live)
                .ToListAsync();
        }

        public async Task<List<Match>> GetUnfinishedMatchesAsync()
        {
            return await _context.Matches
                .Include(c => c.LiveMatches)
                .Include(c => c.Competition)
                .Include(c => c.HomeTeam)
                .Include(c => c.AwayTeam)
                .Where(c => c.MatchStatus != MatchStatus.Finished).AsNoTracking().ToListAsync();
        }

        public async Task<List<Match>> GetFinishedMatchesFromCompetitionAsync(int competitionId)
        {
            return await _context.Matches.Where(c => c.CompetitionId == competitionId && c.MatchStatus == MatchStatus.Finished).ToListAsync();
        }


        public async Task SaveLiveMatchAsync(LiveMatch liveMatch)
        {
            var previousTracking = _context.ChangeTracker.QueryTrackingBehavior;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            try
            {
                var existingLiveMatch = await _context.LiveMatches.FirstOrDefaultAsync(c => c.Id == liveMatch.Id);

                if (existingLiveMatch != null)
                {
                    existingLiveMatch.ExcitmentScore = liveMatch.ExcitmentScore;
                    existingLiveMatch.ScoreLineScore = liveMatch.ScoreLineScore;
                    existingLiveMatch.ShotTargetScore = liveMatch.ShotTargetScore;
                    existingLiveMatch.XGoalsScore = liveMatch.XGoalsScore;
                    existingLiveMatch.TotalFoulsScore = liveMatch.TotalFoulsScore;
                    existingLiveMatch.TotalCardsScore = liveMatch.TotalCardsScore;
                    existingLiveMatch.PossesionScore = liveMatch.PossesionScore;
                    existingLiveMatch.BigChancesScore = liveMatch.BigChancesScore;

                    _context.LiveMatches.Update(existingLiveMatch);
                }
                else
                {
                    _context.LiveMatches.Add(liveMatch);
                }

                await _context.SaveChangesAsync();
            }
            finally
            {
                _context.ChangeTracker.QueryTrackingBehavior = previousTracking;
            }
        }

        public async Task SaveLiveMatchesAsync(List<LiveMatch> liveMatches)
        {
            var previousTracking = _context.ChangeTracker.QueryTrackingBehavior;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            try
            {
                var liveMatchesIds = liveMatches.Select(f => f.Id).ToList();
                var existingLiveMatches = await _context.LiveMatches
                    .Where(f => liveMatchesIds.Contains(f.Id))
                    .ToDictionaryAsync(f => f.Id, f => f);

                var liveMatchesToAdd = new List<LiveMatch>();
                var liveMatchesToUpdate = new List<LiveMatch>();

                foreach (var liveMatch in liveMatches)
                {
                    if (existingLiveMatches.TryGetValue(liveMatch.Id, out var existingMatch))
                    {
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
                        liveMatchesToAdd.Add(liveMatch);
                    }
                }

                if (!liveMatchesToAdd.Any() && !liveMatchesToUpdate.Any())
                {
                    return;
                }

                if (liveMatchesToAdd.Any())
                {
                    _context.LiveMatches.AddRange(liveMatchesToAdd);
                }

                if (liveMatchesToUpdate.Any())
                {
                    _context.LiveMatches.UpdateRange(liveMatchesToUpdate);
                }

                await _context.SaveChangesAsync();
            }
            finally
            {
                _context.ChangeTracker.QueryTrackingBehavior = previousTracking;
            }
        }

        public async Task<LiveMatch?> GetLiveMatchByIdAsync(int id)
        {
            return await _context.LiveMatches.FirstOrDefaultAsync(c => c.Id == id);
        }


        public async Task SaveHeadToHeadMatchesAsync(List<Headtohead> headtoheadMatches)
        {
            if (headtoheadMatches == null || headtoheadMatches.Count == 0)
            {
                return;
            }

            var previousTracking = _context.ChangeTracker.QueryTrackingBehavior;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            try
            {
                var matchId = headtoheadMatches[0].MatchId;

                var existingHeadToHeadMatches = _context.HeadtoheadMatches
                    .Where(f => f.MatchId == matchId);

                _context.HeadtoheadMatches.RemoveRange(existingHeadToHeadMatches);
                _context.HeadtoheadMatches.AddRange(headtoheadMatches);

                await _context.SaveChangesAsync();
            }
            finally
            {
                _context.ChangeTracker.QueryTrackingBehavior = previousTracking;
            }
        }
        #endregion

        #region Rivalry Methods

        public async Task SaveRivalryAsync(Rivalry rivalry)
        {
            var previousTracking = _context.ChangeTracker.QueryTrackingBehavior;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            try
            {
                var existingRivalry = await _context.Rivalries.FirstOrDefaultAsync(c => c.Id == rivalry.Id);

                if (existingRivalry != null)
                {
                    existingRivalry.RivarlyValue = rivalry.RivarlyValue;
                    _context.Rivalries.Update(existingRivalry);
                }
                else
                {
                    _context.Rivalries.Add(rivalry);
                }

                await _context.SaveChangesAsync();
            }
            finally
            {
                _context.ChangeTracker.QueryTrackingBehavior = previousTracking;
            }
        }

        public async Task<Rivalry?> GetRivalryByTeamIdAsync(int teamOneId, int teamTwoId)
        {
            return await _context.Rivalries.Where(c =>
                        (c.TeamOneId == teamOneId && c.TeamTwoId == teamTwoId)
                        || (c.TeamTwoId == teamOneId && c.TeamOneId == teamTwoId)
                        )
                .FirstOrDefaultAsync();
        }


        #endregion
    }
}

