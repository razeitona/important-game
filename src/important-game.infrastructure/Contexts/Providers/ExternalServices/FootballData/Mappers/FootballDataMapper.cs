using important_game.infrastructure.Contexts.Providers.ExternalServices.FootballData.Models;
using important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations.Models;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.FootballData.Mappers;
public static class FootballDataMapper
{
    public static ExternalCompetitionDto MapToExternalCompetition(FootballDataCompetition competition)
    {
        var externalCompetition = new ExternalCompetitionDto();
        externalCompetition.Id = competition.Code!;
        externalCompetition.Name = competition.Name;
        externalCompetition.CurrentSeason = new ExternalSeasonDto();
        externalCompetition.CurrentSeason.Id = competition.CurrentSeason!.Id.ToString();
        externalCompetition.CurrentSeason.StartDate = competition.CurrentSeason.StartDate;
        externalCompetition.CurrentSeason.EndDate = competition.CurrentSeason.EndDate;
        return externalCompetition;
    }


    public static ExternalCompetitionStandingsDto MapToExternalCompetitionStandings(FootballDataStandingsResponse footballDataStandingsResponse)
    {
        var externalStandings = new ExternalCompetitionStandingsDto();

        var leagueStandings = footballDataStandingsResponse.Standings.FirstOrDefault();
        if (leagueStandings == null)
            return externalStandings;

        var standings = new List<ExternalCompetitionStandingRowDto>();
        foreach (var row in leagueStandings.Table)
        {
            var teamStanding = new ExternalCompetitionStandingRowDto();
            teamStanding.Team = MapToExternalTeamDto(row.Team);

            teamStanding.Position = row.Position;
            teamStanding.PlayedGames = row.PlayedGames;
            teamStanding.Won = row.Won;
            teamStanding.Draw = row.Draw;
            teamStanding.Lost = row.Lost;
            teamStanding.GoalsFor = row.GoalsFor;
            teamStanding.GoalsAgainst = row.GoalsAgainst;

            standings.Add(teamStanding);
        }

        externalStandings.Standings = standings;

        return externalStandings;
    }

    public static List<ExternalMatchDto> MapToExternalMatch(IReadOnlyList<FootballDataMatch> footballDataMatches)
    {
        if (footballDataMatches == null || footballDataMatches.Count == 0)
            return [];


        var externalMatches = new List<ExternalMatchDto>();

        foreach (var footballMatch in footballDataMatches)
        {
            if (footballMatch.Score?.FullTime == null)
                continue;

            var externalMatch = new ExternalMatchDto();
            externalMatch.Id = footballMatch.Id.ToString();
            externalMatch.MatchDateUtc = footballMatch.UtcDate;
            externalMatch.HomeTeam = MapToExternalTeamDto(footballMatch.HomeTeam);
            externalMatch.AwayTeam = MapToExternalTeamDto(footballMatch.AwayTeam);
            externalMatch.HomeGoals = footballMatch.Score.FullTime.Home!.Value;
            externalMatch.AwayGoals = footballMatch.Score.FullTime.Away!.Value;

            externalMatch.RoundId = footballMatch.Matchday;
            externalMatch.SeasonId = footballMatch.Season?.Id.ToString();

            externalMatches.Add(externalMatch);
        }

        return externalMatches;
    }

    private static ExternalTeamDto MapToExternalTeamDto(FootballDataTeam footballTeam)
    {
        var team = new ExternalTeamDto();
        team.Id = footballTeam.Id;
        team.Name = footballTeam.Name;
        team.ShortName = footballTeam.ShortName;
        team.ThreeLetterName = footballTeam.Tla;
        return team;
    }
}
