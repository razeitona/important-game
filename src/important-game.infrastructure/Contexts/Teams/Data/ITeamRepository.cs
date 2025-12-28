using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Teams.Data.Entities;

namespace important_game.infrastructure.Contexts.Teams.Data;

/// <summary>
/// Repository interface for Team entity operations.
/// Follows Single Responsibility Principle by focusing only on Team data access.
/// </summary>
public interface ITeamRepository
{
    /// <summary>
    /// Gets all teams.
    /// </summary>
    Task<List<TeamEntity>> GetAllTeamsAsync();

    /// <summary>
    /// Saves a team (insert or update).
    /// </summary>
    Task<TeamEntity> SaveTeamAsync(TeamEntity entity);

    /// <summary>
    /// Saves a team (insert or update).
    /// </summary>
    Task<Team> SaveTeamAsync(Team team);

    /// <summary>
    /// Gets a team by its ID.
    /// </summary>
    Task<Team?> GetTeamByIdAsync(int id);

    /// <summary>
    /// Gets multiple teams by their IDs.
    /// </summary>
    Task<List<Team>> GetTeamsByIdsAsync(List<int> ids);
}
