using important_game.infrastructure.ImportantMatch.Data.Entities;

namespace important_game.infrastructure.ImportantMatch.Data
{
    public interface IExctimentMatchRepository
    {
        Task SaveCompetitionAsync(Competition competition);
        Task<Competition?> GetCompetitionByIdAsync(int id);
        Task<List<Competition>> GetCompetitionsAsync();
        Task<List<Competition>> GetActiveCompetitionsAsync();


        Task<Team> SaveTeamAsync(Team team);
        Task<Team?> GetTeamByIdAsync(int teamId);
        Task<List<Team>> GetTeamsByIdsAsync(List<int> teamIds);


        Task SaveMatchAsync(Match match);
        Task SaveMatchesAsync(List<Match> matches);
        Task<Match?> GetMatchByIdAsync(int matchId);
        Task<List<Match>> GetUpcomingMatchesAsync();
        Task<List<Match>> GetMatchesFromCompetitionAsync(int competitionId);
        Task<List<Match>> GetUpcomingMatchesFromCompetitionAsync(int competitionId);
        Task<List<Match>> GetLiveMatchesFromCompetitionAsync(int competitionId);
        Task<List<Match>> GetUnfinishedMatchesAsync();
        Task<List<Match>> GetFinishedMatchesFromCompetitionAsync(int competitionId);
        Task<List<Match>> GetCompetitionActiveMatchesAsync(int competitionId);


        Task SaveLiveMatchAsync(LiveMatch liveMatch);
        Task SaveLiveMatchesAsync(List<LiveMatch> liveMatches);
        Task<LiveMatch?> GetLiveMatchByIdAsync(int matchId);


        Task SaveHeadToHeadMatchesAsync(List<Headtohead> headtoheadMatches);


        Task SaveRivalryAsync(Rivalry rivalry);
        Task<Rivalry?> GetRivalryByTeamIdAsync(int teamOneId, int teamTwoId);
    }
}
