using Dapper;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Data.Queries;
using important_game.infrastructure.Data.Connections;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Matches.Data;

/// <summary>
/// Dapper-based repository for Match entities with full CRUD and query operations.
/// Handles complex queries involving matches, teams, competitions, and related data.
/// </summary>
[ExcludeFromCodeCoverage]
public class MatchesRepository(IDbConnectionFactory connectionFactory) : IMatchesRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    public async Task<List<UnfinishedMatchDto>> GetUnfinishedMatchesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<UnfinishedMatchDto>(MatchesQueries.SelectUnfinishedMatches);
        return result.ToList();
    }

    public async Task<MatchesEntity> SaveMatchAsync(MatchesEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using var connection = _connectionFactory.CreateConnection();
        var matchId = await connection.ExecuteScalarAsync<int>(MatchesQueries.CheckMatchExists,
            new { entity.HomeTeamId, entity.AwayTeamId, MatchDateUTC = entity.MatchDateUTC.ToString("yyyy-MM-dd HH:mm:ss") });

        if (matchId > 0)
        {
            await connection.ExecuteAsync(MatchesQueries.UpdateMatch, new
            {
                MatchId = matchId,
                entity.CompetitionId,
                entity.SeasonId,
                entity.Round,
                MatchDateUTC = entity.MatchDateUTC.ToString("yyyy-MM-dd HH:mm:ss"),
                entity.HomeTeamId,
                entity.AwayTeamId,
                entity.HomeScore,
                entity.AwayScore,
                entity.IsFinished,
                entity.ExcitmentScore,
                entity.CompetitionScore,
                entity.FixtureScore,
                entity.FormScore,
                entity.GoalsScore,
                entity.CompetitionStandingScore,
                entity.HeadToHeadScore,
                entity.RivalryScore,
                entity.TitleHolderScore,
                UpdatedDateUTC = entity.UpdatedDateUTC.ToString("yyyy-MM-dd HH:mm:ss")
            });
            entity.MatchId = matchId;
            return entity;
        }
        else
        {
            var insertedId = await connection.ExecuteScalarAsync<int>(MatchesQueries.InsertMatch, new
            {
                entity.CompetitionId,
                entity.SeasonId,
                entity.Round,
                MatchDateUTC = entity.MatchDateUTC.ToString("yyyy-MM-dd HH:mm:ss"),
                entity.HomeTeamId,
                entity.AwayTeamId,
                entity.HomeScore,
                entity.AwayScore,
                entity.IsFinished,
                entity.ExcitmentScore,
                entity.CompetitionScore,
                entity.FixtureScore,
                entity.FormScore,
                entity.GoalsScore,
                entity.CompetitionStandingScore,
                entity.HeadToHeadScore,
                entity.RivalryScore,
                entity.TitleHolderScore,
                UpdatedDateUTC = entity.UpdatedDateUTC.ToString("yyyy-MM-dd HH:mm:ss")
            });
            entity.MatchId = insertedId;
            return entity;
        }
    }

    public async Task UpdateMatchCalculatorAsync(MatchCalcsDto entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(MatchesQueries.UpdateMatchCalculators, new
        {
            entity.MatchId,
            entity.HomeForm,
            entity.HomeTeamPosition,
            entity.AwayForm,
            entity.AwayTeamPosition,
            entity.ExcitmentScore,
            entity.CompetitionScore,
            entity.FixtureScore,
            entity.FormScore,
            entity.GoalsScore,
            entity.CompetitionStandingScore,
            entity.HeadToHeadScore,
            entity.RivalryScore,
            entity.TitleHolderScore,
            UpdatedDateUTC = entity.UpdatedDateUTC.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    public async Task<List<MatchDto>> GetAllUpcomingMatchesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<MatchDto>(MatchesQueries.SelectAllUpcomingMatches);
        return result.ToList();
    }

    public async Task<MatchDetailDto?> GetMatchByIdAsync(int matchId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<MatchDetailDto>(MatchesQueries.SelectMatchById, new { MatchId = matchId });
        return result;
    }

    public async Task<MatchDetailDto?> GetMatchByTeamSlugsAsync(string homeSlug, string awaySlug)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<MatchDetailDto>(MatchesQueries.SelectMatchByTeamSlugs, new { HomeSlug = homeSlug.ToLowerInvariant(), AwaySlug = awaySlug.ToLowerInvariant() });
        return result;
    }

    public async Task<DateTimeOffset?> GetTeamLastFinishedMatchDateAsync(int teamId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<DateTimeOffset?>(MatchesQueries.SelectTeamLatestMatchDate, new { TeamId = teamId });
        return result;
    }

    public async Task<List<MatchesEntity>> GetRecentMatchesForTeamAsync(int teamId, int numberOfMatches)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<MatchesEntity>(MatchesQueries.SelectRecentMatchesForTeam,
            new { TeamId = teamId, NumberOfMatches = numberOfMatches });
        return result.ToList();
    }

    #region Head To Head
    public async Task<List<HeadToHeadDto>> GetHeadToHeadMatchesAsync(int teamOneId, int teamTwoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<HeadToHeadDto>(MatchesQueries.SelectHeadToHeadMatches,
        new { TeamOneId = teamOneId, TeamTwoId = teamTwoId });

        return result.ToList();
    }
    #endregion

    #region Rivalry
    public async Task<RivalryEntity?> GetRivalryAsync(int teamOneId, int teamTwoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<RivalryEntity>(
            RivalryQueries.SelectRivalryByTeamId,
            new
            {
                TeamOneId = teamOneId,
                TeamTwoId = teamTwoId
            });
        return result;
    }
    #endregion
}
