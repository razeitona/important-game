using important_game.infrastructure.ImportantMatch.Data.Entities;

namespace important_game.infrastructure.ImportantMatch.Data
{
    public interface IExctimentMatchRepository
    {
        Task SaveCompetitionAsync(Competition competition);
        Competition? GetCompetitionById(int id);
        List<Competition> GetCompetitions();
        List<Competition> GetActiveCompetitions();


        Task<Team> SaveTeamAsync(Team team);
        Task<Team?> GetTeamById(int teamId);
        Task<List<Team>> GetTeamsByIds(List<int> teamIds);


        Task SaveMatchAsync(Match match);
        Task SaveMatchesAsync(List<Match> matches);
        Match? GetMatchById(int matchId);
        Task<List<Match>> GetUpcomingMatchesAsync();
        List<Match> GetMatchesFromCompetition(int competitionId);
        List<Match> GetUpcomingMatchesFromCompetition(int competitionId);
        List<Match> GetLiveMatchesFromCompetition(int competitionId);
        List<Match> GetLiveMatches();
        List<Match> GetFinishedMatchesFromCompetition(int competitionId);
        List<Match> GetCompetitionActiveMatches(int competitionId);


        Task SaveLiveMatchAsync(LiveMatch liveMatch);
        Task SaveLiveMatchesAsync(List<LiveMatch> liveMatches);
        Task<LiveMatch?> GetLiveMatchById(int matchId);


        Task SaveHeadToHeadMatchesAsync(List<Headtohead> headtoheadMatches);


        Task SaveRivalryAsync(Rivalry rivalry);
        Rivalry? GetRivalryByTeamId(int teamOneId, int teamTwoId);
    }
}
