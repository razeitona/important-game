using important_game.infrastructure.FootballData.Models;

namespace important_game.infrastructure.FootballData;

public interface IFootballDataIntegration
{
    Task<FootballDataCompetition?> GetCompetitionAsync(string competitionId, CancellationToken cancellationToken = default);

    Task<FootballDataStandingsResponse?> GetCompetitionStandingsAsync(int competitionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FootballDataMatch>> GetUpcomingMatchesAsync(int competitionId, int daysAhead = 7, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FootballDataMatch>> GetTeamMatchesAsync(int teamId, string status = "FINISHED", int limit = FootballDataConstants.DefaultRecentMatchesLimit, CancellationToken cancellationToken = default);

    Task<FootballDataHeadToHeadResponse?> GetHeadToHeadAsync(int matchId, int limit = FootballDataConstants.DefaultRecentMatchesLimit, CancellationToken cancellationToken = default);

    Task<FootballDataMatch?> GetMatchAsync(int matchId, CancellationToken cancellationToken = default);
}
