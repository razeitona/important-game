using FuzzySharp;
using important_game.infrastructure.Contexts.Competitions.Data;
using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Contexts.Providers.Data;
using important_game.infrastructure.Contexts.Providers.Data.Entities;
using important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations;
using important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations.Models;
using important_game.infrastructure.Contexts.Teams.Data;
using important_game.infrastructure.Contexts.Teams.Data.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices;

public interface IExternalCompetitionSyncService
{
    Task SyncCompetitionsAsync(CancellationToken cancellationToken = default);
}

[ExcludeFromCodeCoverage]
public class ExternalCompetitionSyncService : IExternalCompetitionSyncService
{
    private const string CurrentSeasonYear = "2025/2026";

    private readonly IIntegrationProviderFactory _integrationProvider;
    private readonly IExternalProvidersRepository _externalIntegrationRepository;
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger<ExternalCompetitionSyncService> _logger;

    public ExternalCompetitionSyncService(
        IIntegrationProviderFactory integrationProvider,
        IExternalProvidersRepository externalIntegrationRepository,
        ITeamRepository teamRepository,
        ICompetitionRepository competitionRepository,
        ILogger<ExternalCompetitionSyncService> logger)
    {
        _integrationProvider = integrationProvider ?? throw new ArgumentNullException(nameof(integrationProvider));
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        _externalIntegrationRepository = externalIntegrationRepository ?? throw new ArgumentNullException(nameof(externalIntegrationRepository));
        _competitionRepository = competitionRepository ?? throw new ArgumentNullException(nameof(competitionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SyncCompetitionsAsync(CancellationToken cancellationToken = default)
    {
        var activeCompetitions = await _competitionRepository.GetActiveCompetitionsAsync();
        foreach (var competition in activeCompetitions)
        {
            var provider = _integrationProvider.GetBest<IExternalCompetitionProvider>();

            var externalCompetitionId = await IdentifyExternalCompetitionAsync(provider.Id, competition.CompetitionId);
            if (string.IsNullOrWhiteSpace(externalCompetitionId))
                continue;

            await SyncCompetitionDataAsync(competition.CompetitionId, externalCompetitionId, provider, cancellationToken);
        }
    }

    private async Task SyncCompetitionDataAsync(int competitionId, string externalCompetitionId, IExternalCompetitionProvider externalProvider, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting synchronization of competition {CompetitionId} from external source {ExternalCompetitionId}", competitionId, externalCompetitionId);

            var competitionInfo = await externalProvider.GetCompetitionAsync(externalCompetitionId, cancellationToken);

            await SaveExternalProviderCompetitionMappingAsync(externalProvider.Id, competitionId, externalCompetitionId);

            var currentSeason = await SaveCompetitionSeasonAsync(competitionId);

            await SaveExternalProviderSeasonMappingAsync(externalProvider.Id, currentSeason.SeasonId, competitionInfo.CurrentSeason!.Id);

            if (currentSeason.SyncStandingsDate.HasValue && currentSeason.SyncStandingsDate.Value.AddHours(12) > DateTime.UtcNow)
            {
                _logger.LogInformation("Skipping standings synchronization for competition {CompetitionId} as it was recently synchronized", competitionId);
                return;
            }

            await SyncCompetitionStandingsAsync(competitionId, externalCompetitionId, currentSeason.SeasonId, externalProvider, cancellationToken);

            _logger.LogInformation("Successfully synchronized competition {CompetitionId}", competitionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing competition {CompetitionId}", competitionId);
            throw;
        }
    }

    private async Task SyncCompetitionStandingsAsync(int competitionId, string externalCompetitionId, int seasonId, IExternalCompetitionProvider externalProvider, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting synchronization of competition {CompetitionId} standings from external source {ExternalCompetitionId}", competitionId, externalCompetitionId);

            var standings = await externalProvider.GetCompetitionStandingsAsync(externalCompetitionId, cancellationToken);
            if (standings == null)
            {
                _logger.LogWarning("Failed to retrieve standings for competition {ExternalCompetitionId}", externalCompetitionId);
                return;
            }

            await SyncTeamsAndTableAsync(externalProvider.Id, competitionId, seasonId, standings);

            await _competitionRepository.UpdateCompetitionSeasonStandingDateAsync(seasonId, DateTime.UtcNow);

            _logger.LogInformation("Successfully synchronized competition {CompetitionId}", competitionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing competition {CompetitionId}", competitionId);
            throw;
        }
    }

    private async Task<string> IdentifyExternalCompetitionAsync(int providerId, int competitionId)
    {
        var externalCompetitionInfo = await _externalIntegrationRepository.GetExternalIntegrationCompetitionAsync(providerId, competitionId);
        if (externalCompetitionInfo == null)
            return string.Empty;

        return externalCompetitionInfo.ExternalCompetitionId;
    }

    private async Task SaveExternalProviderCompetitionMappingAsync(int providerId, int internalCompetitionId, string externalCompetitionId)
    {
        var mapping = new ExternalProviderCompetitionsEntity
        {
            ProviderId = providerId,
            InternalCompetitionId = internalCompetitionId,
            ExternalCompetitionId = externalCompetitionId
        };

        await _externalIntegrationRepository.SaveExternalIntegrationCompetitionAsync(mapping);
    }

    private async Task SaveExternalProviderSeasonMappingAsync(int providerId, int seasonId, string externalSeasonId)
    {
        var mapping = new ExternalProviderCompetitionSeasonsEntity
        {
            ProviderId = providerId,
            InternalSeasonId = seasonId,
            ExternalSeasonId = externalSeasonId
        };

        await _externalIntegrationRepository.SaveExternalProviderCompetitionSeasonAsync(mapping);
    }

    private async Task<CompetitionSeasonsEntity> SaveCompetitionSeasonAsync(int competitionId)
    {
        var existingSeason = await _competitionRepository.GetCompetitionSeasonByCompetitionAndYearAsync(competitionId, CurrentSeasonYear);

        if (existingSeason != null)
        {
            return existingSeason;
        }

        var newSeason = new CompetitionSeasonsEntity
        {
            CompetitionId = competitionId,
            SeasonYear = CurrentSeasonYear,
            TitleHolderId = default
        };

        return await _competitionRepository.SaveCompetitionSeasonAsync(newSeason);
    }

    private async Task SyncTeamsAndTableAsync(int providerId, int competitionId, int seasonId, ExternalCompetitionStandingsDto competitionStandings)
    {
        if (competitionStandings.Standings.Count == 0)
        {
            _logger.LogWarning("No standings data available for competition {CompetitionId}", competitionId);
            return;
        }

        var tableRows = new List<CompetitionTableEntity>();

        var listOfTeams = await _teamRepository.GetAllTeamsAsync();

        foreach (var row in competitionStandings.Standings)
        {
            if (row.Team == null)
                continue;

            // Identify or create team
            var teamFound = await FindInternalTeamAsyync(listOfTeams, providerId, row.Team.Id);
            teamFound ??= IdentifyExistingTeam(listOfTeams, row.Team.Name!, row.Team.ShortName);
            teamFound ??= await CreateTeamAsync(row.Team);
            await SaveExternalTeamMappingAsync(providerId, teamFound.Id, row.Team.Id);

            tableRows.Add(new CompetitionTableEntity
            {
                CompetitionId = competitionId,
                SeasonId = seasonId,
                TeamId = teamFound.Id,
                Position = row.Position,
                Points = row.Points,
                Matches = row.PlayedGames,
                Wins = row.Won,
                Draws = row.Draw,
                Losses = row.Lost,
                GoalsFor = row.GoalsFor,
                GoalsAgainst = row.GoalsAgainst
            });
        }

        await SaveCompetitionTableAsync(competitionId, seasonId, tableRows);
    }

    private static TeamEntity? IdentifyExistingTeam(List<TeamEntity> listOfTeams, string name, string? shortName)
    {
        if (listOfTeams.Count == 0)
            return null;

        var ranked = listOfTeams.Select(c => new
        {
            Team = c,
            Score = Math.Max(
                Fuzz.Ratio(c.NormalizedName ?? c.Name.Normalize(), name.Normalize()),
                Fuzz.Ratio(c.ShortName?.Normalize() ?? c.Name.Normalize(), shortName?.Normalize() ?? "UNKNOWN_NAME"))
        })
        .Where(c => c.Score > 75)
        .OrderByDescending(x => x.Score)
        .Take(5);

        var selectedTeam = ranked.FirstOrDefault();

        return selectedTeam?.Team ?? null;
    }

    private async Task<TeamEntity?> FindInternalTeamAsyync(List<TeamEntity> listOfTeams, int providerId, int externalTeamId)
    {
        var externalTeamInfo = await _externalIntegrationRepository.GetExternalIntegrationTeamByExternalIdAsync(providerId, externalTeamId);
        if (externalTeamInfo == null)
            return null;

        var teamFound = listOfTeams.FirstOrDefault(c => c.Id == externalTeamInfo.InternalTeamId);
        if (teamFound == null)
            return null;

        return teamFound;
    }

    private async Task<TeamEntity> CreateTeamAsync(ExternalTeamDto team)
    {
        var teamEntity = new TeamEntity
        {
            Name = team.Name!,
            ShortName = team.ShortName,
            ThreeLetterName = team.ThreeLetterName,
            NormalizedName = team.Name!.Normalize()
        };

        teamEntity = await _teamRepository.SaveTeamAsync(teamEntity);
        return teamEntity;
    }

    private async Task SaveExternalTeamMappingAsync(int providerId, int internalTeamId, int externalTeamId)
    {
        var mapping = new ExternalProviderTeamsEntity
        {
            ProviderId = providerId,
            InternalTeamId = internalTeamId,
            ExternalTeamId = externalTeamId.ToString()
        };

        await _externalIntegrationRepository.SaveExternalIntegrationTeamAsync(mapping);
    }

    private async Task SaveCompetitionTableAsync(int competitionId, int seasonId, List<CompetitionTableEntity> tableRows)
    {
        await _competitionRepository.DeleteCompetitionTableByCompetitionAndSeasonAsync(competitionId, seasonId);
        await _competitionRepository.SaveCompetitionTableAsync(tableRows);
    }
}
