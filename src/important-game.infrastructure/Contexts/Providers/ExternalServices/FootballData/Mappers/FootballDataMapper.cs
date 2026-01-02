using important_game.infrastructure.Contexts.Providers.ExternalServices.FootballData.Models;
using important_game.infrastructure.Contexts.Providers.ExternalServices.Integrations.Models;

namespace important_game.infrastructure.Contexts.Providers.ExternalServices.FootballData.Mappers;
public static class FootballDataMapper
{
    public static ExternalCompetitionDto? MapToExternalCompetition(FootballDataCompetition? competition)
    {
        if (competition == null)
            return null;

        var externalCompetition = new ExternalCompetitionDto();
        externalCompetition.Id = competition.Code!;
        externalCompetition.Name = competition.Name;
        externalCompetition.CurrentSeason = MapToExternalSeason(competition.CurrentSeason);
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
            teamStanding.Points = row.Points;
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
            externalMatch.HomeGoals = footballMatch.Score?.FullTime?.Home ?? null;
            externalMatch.AwayGoals = footballMatch.Score?.FullTime?.Away ?? null;

            externalMatch.Round = footballMatch.Matchday;
            externalMatch.Competition = MapToExternalCompetition(footballMatch.Competition);
            externalMatch.Season = MapToExternalSeason(footballMatch.Season);

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

    private static ExternalSeasonDto? MapToExternalSeason(FootballDataSeason? dataSeason)
    {
        if (dataSeason == null)
            return null;

        var season = new ExternalSeasonDto();
        season.Id = dataSeason.Id.ToString();
        season.StartDate = dataSeason.StartDate;
        season.EndDate = dataSeason.EndDate;

        return season;
    }
}
