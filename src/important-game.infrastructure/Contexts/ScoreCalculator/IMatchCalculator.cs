using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Data.Entities;

namespace important_game.infrastructure.Contexts.ScoreCalculator;
public interface IMatchCalculator
{
    MatchCalcsDto? CalculateMatchScore(UnfinishedMatchDto match, List<CompetitionTableEntity> competitionTable, RivalryEntity? rivarlyInformation, List<HeadToHeadDto> headToHeadMatches);
}
