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

    public async Task<MatchesEntity> SaveFinishedMatchAsync(MatchesEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using var connection = _connectionFactory.CreateConnection();
        var exists = await connection.ExecuteScalarAsync<int>(MatchesQueries.CheckMatchExists,
            new { entity.HomeTeamId, entity.AwayTeamId, entity.MatchDateUTC }) > 0;

        if (exists)
        {
            await connection.ExecuteAsync(MatchesQueries.UpdateFinishedMatch, new
            {
                entity.MatchId,
                entity.CompetitionId,
                entity.SeasonId,
                entity.MatchDateUTC,
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
                entity.UpdatedDateUTC
            });

            return entity;
        }
        else
        {
            var insertedId = await connection.ExecuteScalarAsync<int>(MatchesQueries.InsertFinishedMatch, new
            {
                entity.CompetitionId,
                entity.SeasonId,
                entity.Round,
                entity.MatchDateUTC,
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
                entity.UpdatedDateUTC
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
            entity.AwayForm,
            entity.ExcitmentScore,
            entity.CompetitionScore,
            entity.FixtureScore,
            entity.FormScore,
            entity.GoalsScore,
            entity.CompetitionStandingScore,
            entity.HeadToHeadScore,
            entity.RivalryScore,
            entity.TitleHolderScore,
            entity.UpdatedDateUTC
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
