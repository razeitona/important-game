using FuzzySharp;
using important_game.infrastructure.Contexts.Competitions.Data;
using important_game.infrastructure.Contexts.Matches.Data;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Providers.Data;
using important_game.infrastructure.Contexts.Providers.Data.Entities;
using important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations;
using important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations.Models;
using important_game.infrastructure.Contexts.Teams.Data;
using important_game.infrastructure.Contexts.Teams.Data.Entities;
using important_game.infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices;

public interface IExternalMatchesSyncService
{
    Task SyncFinishedMatchesAsync(CancellationToken cancellationToken = default);
    Task SyncUpcomingMatchesAsync(CancellationToken cancellationToken = default);
}

[ExcludeFromCodeCoverage]
public class ExternalMatchesSyncService : IExternalMatchesSyncService
{
    private readonly IIntegrationProviderFactory _integrationProvider;
    private readonly IExternalProvidersRepository _externalProvidersRepository;
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IMatchesRepository _matchesRepository;
    private readonly ILogger<ExternalMatchesSyncService> _logger;

    public ExternalMatchesSyncService(
        IIntegrationProviderFactory integrationProvider,
        IExternalProvidersRepository externalProvidersRepository,
        ITeamRepository teamRepository,
        ICompetitionRepository competitionRepository,
        IMatchesRepository matchesRepository,
        ILogger<ExternalMatchesSyncService> logger)
    {
        _integrationProvider = integrationProvider ?? throw new ArgumentNullException(nameof(integrationProvider));
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        _externalProvidersRepository = externalProvidersRepository ?? throw new ArgumentNullException(nameof(externalProvidersRepository));
        _competitionRepository = competitionRepository ?? throw new ArgumentNullException(nameof(competitionRepository));
        _matchesRepository = matchesRepository ?? throw new ArgumentNullException(nameof(matchesRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SyncFinishedMatchesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = _integrationProvider.GetBest<IExternalMatchProvider>();
            var teamsInCompetitions = await GetTeamsFromCompetitionTableAsync();

            var listOfTeams = await _teamRepository.GetAllTeamsAsync();
            var externalIntegrationTeams = await _externalProvidersRepository.GetExternalIntegrationTeamsByIntegrationAsync(provider.Id);

            foreach (var teamId in teamsInCompetitions)
            {
                var externalTeam = externalIntegrationTeams.FirstOrDefault(c => c.InternalTeamId == teamId);
                if (externalTeam == null)
                    continue;

                await SyncTeamFinishedMatchesAsync(provider, teamId, externalTeam.ExternalTeamId, listOfTeams, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing matches");
            throw;
        }
    }

    public async Task SyncUpcomingMatchesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = _integrationProvider.GetBest<IExternalMatchProvider>();
            var activeCompetitions = await _competitionRepository.GetActiveCompetitionsAsync();

            var listOfTeams = await _teamRepository.GetAllTeamsAsync();
            foreach (var competition in activeCompetitions)
            {
                var currentSeason = await _competitionRepository.GetLatestCompetitionSeasonAsync(competition.CompetitionId);
                if (currentSeason == null)
                    continue;

                if (currentSeason.SyncMatchesDate.HasValue && currentSeason.SyncMatchesDate.Value.AddHours(12) > DateTime.UtcNow)
                {
                    return;
                }

                var externalCompetition = await _externalProvidersRepository.GetExternalIntegrationCompetitionAsync(provider.Id, competition.CompetitionId);
                if (externalCompetition == null)
                    continue;

                await SyncCompetitionUpcomingMatchesAsync(provider, externalCompetition.InternalCompetitionId, externalCompetition.ExternalCompetitionId, listOfTeams, cancellationToken);

                await _competitionRepository.UpdateCompetitionSeasonMatchesDateAsync(currentSeason.SeasonId, DateTime.UtcNow);
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing matches");
            throw;
        }
    }

    private async Task<List<int>> GetTeamsFromCompetitionTableAsync()
    {
        var activeCompetitions = await _competitionRepository.GetActiveCompetitionsAsync();
        var uniqueTeamIds = new HashSet<int>();

        foreach (var competition in activeCompetitions)
        {
            var latestSeason = await _competitionRepository.GetLatestCompetitionSeasonAsync(competition.CompetitionId);
            if (latestSeason == null)
                continue;

            var tableData = await _competitionRepository.GetCompetitionTableAsync(competition.CompetitionId, latestSeason.SeasonId);

            foreach (var row in tableData)
            {
                uniqueTeamIds.Add(row.TeamId);
            }
        }

        return uniqueTeamIds.ToList();
    }

    private async Task SyncTeamFinishedMatchesAsync(
        IExternalMatchProvider provider,
        int internalTeamId,
        string externalTeamId,
        List<TeamEntity> listOfTeams,
        CancellationToken cancellationToken)
    {
        try
        {
            var lastFinishedMatch = await _matchesRepository.GetTeamLastFinishedMatchDateAsync(internalTeamId);

            DateTimeOffset dateFrom = DateTimeOffset.UtcNow.AddYears(-2);
            if (lastFinishedMatch.HasValue)
            {
                dateFrom = lastFinishedMatch.Value.AddDays(1);
            }

            DateTimeOffset dateTo = dateFrom.AddDays(100);
            DateTimeOffset currentDate = DateTimeOffset.UtcNow;

            while (true)
            {
                var externalMatches = await provider.GetTeamFinishedMatchesAsync(
                    externalTeamId,
                    dateFrom,
                    dateTo,
                    100,
                    cancellationToken);

                if (externalMatches.Count > 0)
                    await ProcessAndSaveFinishedMatchesAsync(provider.Id, externalMatches, listOfTeams);

                dateFrom = dateTo.AddDays(1);
                dateTo = dateFrom.AddDays(100);

                if (currentDate < dateFrom)
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing matches for team {TeamId}", internalTeamId);
            throw;
        }
    }

    private async Task SyncCompetitionUpcomingMatchesAsync(
        IExternalMatchProvider provider,
        int internalCompetitionId,
        string externalCompetitionId,
        List<TeamEntity> listOfTeams,
        CancellationToken cancellationToken)
    {
        try
        {
            DateTimeOffset dateFrom = DateTimeOffset.UtcNow;
            DateTimeOffset dateTo = dateFrom.AddDays(30);

            var externalMatches = await provider.GetCompetitionUpcomingMatchesAsync(
                externalCompetitionId,
                dateFrom,
                dateTo,
                cancellationToken);

            await ProcessAndSaveUpcomingMatchesAsync(provider.Id, internalCompetitionId, externalMatches, listOfTeams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing matches for team {CompetitionId}", internalCompetitionId);
        }
    }

    private async Task ProcessAndSaveFinishedMatchesAsync(
        int providerId,
        List<ExternalMatchDto> externalMatches,
        List<TeamEntity> listOfTeams)
    {
        var externalProviderMatches = await _externalProvidersRepository.GetExternalProviderMatchesByProviderAsync(providerId);

        foreach (var externalMatch in externalMatches)
        {
            // Identify or create home team
            var homeTeam = await FindOrCreateTeamAsync(providerId, externalMatch.HomeTeam, listOfTeams);
            if (homeTeam == null)
                continue;

            // Identify or create away team
            var awayTeam = await FindOrCreateTeamAsync(providerId, externalMatch.AwayTeam, listOfTeams);
            if (awayTeam == null)
                continue;

            // Get competition for this match (based on external season)
            var competitionId = await IdentifyCompetitionAsync(providerId, externalMatch.Competition);
            var seasonId = await IdentifySeasonAsync(providerId, externalMatch.Season);

            // Create match entity
            var match = new MatchesEntity
            {
                CompetitionId = competitionId,
                SeasonId = seasonId,
                Round = externalMatch.Round,
                MatchDateUTC = externalMatch.MatchDateUtc,
                HomeTeamId = homeTeam.Id,
                AwayTeamId = awayTeam.Id,
                HomeScore = externalMatch.HomeGoals,
                AwayScore = externalMatch.AwayGoals,
                IsFinished = true,
                UpdatedDateUTC = DateTimeOffset.UtcNow
            };

            match = await _matchesRepository.SaveMatchAsync(match);

            // Save external match mapping
            await SaveExternalMatchMappingAsync(providerId, match.MatchId, externalMatch.Id);
        }
    }

    private async Task ProcessAndSaveUpcomingMatchesAsync(
        int providerId,
        int internalCompetitionId,
        List<ExternalMatchDto> externalMatches,
        List<TeamEntity> listOfTeams)
    {
        foreach (var externalMatch in externalMatches)
        {
            // Identify or create home team
            var homeTeam = await FindOrCreateTeamAsync(providerId, externalMatch.HomeTeam, listOfTeams);
            if (homeTeam == null)
                continue;

            // Identify or create away team
            var awayTeam = await FindOrCreateTeamAsync(providerId, externalMatch.AwayTeam, listOfTeams);
            if (awayTeam == null)
                continue;

            // Get competition for this match (based on external season)
            var seasonId = await IdentifySeasonAsync(providerId, externalMatch.Season);

            // Create match entity
            var match = new MatchesEntity
            {
                CompetitionId = internalCompetitionId,
                SeasonId = seasonId,
                Round = externalMatch.Round,
                MatchDateUTC = externalMatch.MatchDateUtc,
                HomeTeamId = homeTeam.Id,
                AwayTeamId = awayTeam.Id,
                HomeScore = externalMatch.HomeGoals,
                AwayScore = externalMatch.AwayGoals,
                IsFinished = false,
                UpdatedDateUTC = DateTimeOffset.UtcNow
            };

            match = await _matchesRepository.SaveMatchAsync(match);

            // Save external match mapping
            await SaveExternalMatchMappingAsync(providerId, match.MatchId, externalMatch.Id);
        }
    }

    private async Task<TeamEntity?> FindOrCreateTeamAsync(
        int providerId,
        ExternalTeamDto externalTeam,
        List<TeamEntity> listOfTeams)
    {
        // Try to find by external ID mapping
        var internalTeam = await FindInternalTeamByExternalIdAsync(providerId, externalTeam.Id, listOfTeams);
        if (internalTeam != null)
        {
            EnrichExistingTeam(internalTeam, listOfTeams, externalTeam.Name, externalTeam.ShortName, externalTeam.ThreeLetterName);
            await _teamRepository.UpdateTeamAsync(internalTeam);
            return internalTeam;
        }

        //// Try to find by fuzzy matching
        internalTeam = IdentifyExistingTeam(listOfTeams, externalTeam.Name, externalTeam.ShortName);

        if (internalTeam != null)
        {
            await SaveExternalTeamMappingAsync(providerId, internalTeam.Id, externalTeam.Id);
            return internalTeam;
        }

        // Create new team
        internalTeam = await CreateTeamAsync(externalTeam);
        if (internalTeam != null)
        {
            await SaveExternalTeamMappingAsync(providerId, internalTeam.Id, externalTeam.Id);
            listOfTeams.Add(internalTeam);
        }

        return internalTeam;
    }

    private async Task<TeamEntity?> FindInternalTeamByExternalIdAsync(int providerId, int externalTeamId, List<TeamEntity> listOfTeams)
    {
        var externalTeamInfo = await _externalProvidersRepository.GetExternalIntegrationTeamByExternalIdAsync(providerId, externalTeamId);
        if (externalTeamInfo == null)
            return null;

        var teamFound = listOfTeams.FirstOrDefault(c => c.Id == externalTeamInfo.InternalTeamId);
        return teamFound;
    }

    private static TeamEntity? IdentifyExistingTeam(List<TeamEntity> listOfTeams, string? name, string? shortName)
    {
        if (listOfTeams.Count == 0 || string.IsNullOrWhiteSpace(name))
            return null;

        var ranked = listOfTeams
            .Select(c => new
            {
                Team = c,
                Score = Math.Max(
                    Fuzz.Ratio(c.NormalizedName ?? c.Name.Normalize(), name.Normalize()),
                    Fuzz.Ratio(c.ShortName?.Normalize() ?? c.Name.Normalize(), shortName?.Normalize() ?? "UNKNOWN_NAME"))
            })
            .Where(c => c.Score > 75)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        return ranked?.Team;
    }

    private static void EnrichExistingTeam(TeamEntity? teamFound, List<TeamEntity> listOfTeams, string name, string? shortName, string? threeLetterName)
    {
        if (listOfTeams.Count == 0)
            return;

        if (teamFound != null)
        {
            teamFound.Name = name;
            teamFound.ShortName = shortName ?? name;
            teamFound.ThreeLetterName = threeLetterName;
            teamFound.NormalizedName = name.Normalize();
            teamFound.SlugName = SlugHelper.GenerateSlug(name);
            return;
        }

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
        if (teamFound == null && selectedTeam?.Team != null)
        {
            teamFound = selectedTeam.Team;
            teamFound.Name = name;
            teamFound.ShortName = shortName ?? name;
            teamFound.ThreeLetterName = threeLetterName;
            teamFound.NormalizedName = name.Normalize();
            teamFound.SlugName = SlugHelper.GenerateSlug(name);
        }
    }

    private async Task<TeamEntity?> CreateTeamAsync(ExternalTeamDto externalTeam)
    {
        if (externalTeam == null)
            return null;

        try
        {
            var teamEntity = new TeamEntity
            {
                Name = externalTeam.Name ?? "Unknown Team",
                ShortName = externalTeam.ShortName,
                ThreeLetterName = externalTeam.ThreeLetterName,
                NormalizedName = (externalTeam.Name ?? "Unknown Team").Normalize(),
                SlugName = SlugHelper.GenerateSlug(externalTeam.Name!)
            };

            teamEntity = await _teamRepository.SaveTeamAsync(teamEntity);
            return teamEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating team from external provider");
            return null;
        }
    }

    private async Task<int?> IdentifySeasonAsync(int providerId, ExternalSeasonDto? externalSeason)
    {
        if (externalSeason == null || string.IsNullOrWhiteSpace(externalSeason.Id))
            return null;

        try
        {
            var seasonMapping = await _externalProvidersRepository.GetExternalProviderCompetitionSeasonByExternalIdAsync(providerId, externalSeason.Id);
            if (seasonMapping == null)
                return null;

            return seasonMapping.InternalSeasonId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error identifying season for external season {ExternalSeasonId}", externalSeason.Id);
            return null;
        }
    }

    private async Task<int?> IdentifyCompetitionAsync(int providerId, ExternalCompetitionDto? externalCompetition)
    {
        if (externalCompetition == null || string.IsNullOrWhiteSpace(externalCompetition.Id))
            return null;

        try
        {
            var competitionMapping = await _externalProvidersRepository.GetExternalCompetitionByExternalIdAsync(providerId, externalCompetition.Id);
            if (competitionMapping == null)
                return null;

            return competitionMapping.InternalCompetitionId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error identifying season for external season {ExternalSeasonId}", externalCompetition.Id);
            return null;
        }
    }

    private async Task SaveExternalTeamMappingAsync(int providerId, int internalTeamId, int externalTeamId)
    {
        try
        {
            var mapping = new ExternalProviderTeamsEntity
            {
                ProviderId = providerId,
                InternalTeamId = internalTeamId,
                ExternalTeamId = externalTeamId.ToString()
            };

            await _externalProvidersRepository.SaveExternalIntegrationTeamAsync(mapping);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error saving external team mapping");
        }
    }

    private async Task SaveExternalMatchMappingAsync(int providerId, int internalMatchId, string externalMatchId)
    {
        try
        {
            var mapping = new ExternalProviderMatchesEntity
            {
                ProviderId = providerId,
                InternalMatchId = internalMatchId,
                ExternalMatchId = externalMatchId
            };

            await _externalProvidersRepository.SaveExternalProviderMatchAsync(mapping);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error saving external match mapping");
        }
    }
}
