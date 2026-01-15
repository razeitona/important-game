using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Matches.Enums;
using important_game.infrastructure.Contexts.Matches.Models;
using important_game.infrastructure.Contexts.Matches.Utils;

namespace important_game.infrastructure.Contexts.Matches.Mappers;
internal static class MatchMapper
{
    internal static MatchDetailViewModel MapToMatchDetail(MatchDetailDto matchDetailDto)
    {
        var matchDetail = new MatchDetailViewModel();
        matchDetail.MatchId = matchDetailDto.MatchId;
        matchDetail.CompetitionId = matchDetailDto.CompetitionId;
        matchDetail.CompetitionName = matchDetailDto.CompetitionName;
        matchDetail.CompetitionPrimaryColor = matchDetailDto.CompetitionPrimaryColor;
        matchDetail.CompetitionBackgroundColor = matchDetailDto.CompetitionBackgroundColor;
        matchDetail.MatchDateUTC = matchDetailDto.MatchDateUTC;
        matchDetail.ExcitmentScore = matchDetailDto.ExcitmentScore;
        matchDetail.LiveExcitementScore = matchDetailDto.LiveExcitementScore;
        matchDetail.ScoreLineScore = matchDetailDto.ScoreLineScore;
        matchDetail.XGoalsScore = matchDetailDto.XGoalsScore;
        matchDetail.TotalFoulsScore = matchDetailDto.TotalFoulsScore;
        matchDetail.TotalCardsScore = matchDetailDto.TotalCardsScore;
        matchDetail.PossessionScore = matchDetailDto.PossessionScore;
        matchDetail.BigChancesScore = matchDetailDto.BigChancesScore;
        matchDetail.SeasonId = matchDetailDto.SeasonId;

        matchDetail.HomeTeamId = matchDetailDto.HomeTeamId;
        matchDetail.HomeTeamName = matchDetailDto.HomeTeamName;
        matchDetail.HomeTeamForm = ParseTeamForm(matchDetailDto.HomeTeamForm);
        matchDetail.HomeTeamTablePosition = matchDetailDto.HomeTeamTablePosition;

        matchDetail.AwayTeamId = matchDetailDto.AwayTeamId;
        matchDetail.AwayTeamName = matchDetailDto.AwayTeamName;
        matchDetail.AwayTeamForm = ParseTeamForm(matchDetailDto.AwayTeamForm);
        matchDetail.AwayTeamTablePosition = matchDetailDto.AwayTeamTablePosition;

        matchDetail.ExcitmentScoreDetail = SetupExcitmentScoreDetail(matchDetailDto);
        matchDetail.Description = BuildSentence(matchDetail.ExcitmentScoreDetail);
        matchDetail.IsRivalry = matchDetailDto.RivalryScore > 0d;
        matchDetail.HasTitleHolder = matchDetailDto.TitleHolderScore > 0d;

        return matchDetail;
    }

    private static List<MatchResultType> ParseTeamForm(string? homeTeamForm)
    {
        if (string.IsNullOrWhiteSpace(homeTeamForm))
            return [];

        var matchResults = new List<MatchResultType>();

        foreach (var form in homeTeamForm.Split(','))
        {
            if (Enum.TryParse<MatchResultType>(form, out var result))
            {
                matchResults.Add(result);
            }
        }

        return matchResults;
    }

    private static Dictionary<string, (bool Show, double Value)> SetupExcitmentScoreDetail(MatchDetailDto match)
    {
        //if (liveData != null)
        //{
        //    return new Dictionary<string, (bool Show, double Value)>
        //        {
        //            { "Score Line", (true,liveData.ScoreLineScore) },
        //            { "xGoals", (true,liveData.XGoalsScore) },
        //            { "Fouls", (true,liveData.TotalFoulsScore) },
        //            { "Cards", (true,liveData.TotalCardsScore) },
        //            { "Possession", (true,liveData.PossesionScore) },
        //            { "Big chances", (true, liveData.BigChancesScore) },
        //        };
        //}

        var scoreDetail = new Dictionary<string, (bool Show, double Value)>
            {
                { "League Coeficient", (true,match.CompetitionScore) },
                { "League Standings", (true, match.CompetitionStandingScore) },
                { "Fixture Importance", (true, match.FixtureScore) },
                { "Teams Form", (true, match.FormScore) },
                { "Teams Goals", (true, match.GoalsScore) },
                { "Head to head", (true, match.HeadToHeadScore) },
                { "Rivalry", (false, match.RivalryScore) },
                { "Title Holder", (false, match.TitleHolderScore) },
            };

        return scoreDetail;
    }

    private static string BuildSentence(Dictionary<string, (bool show, double value)> excitmentScoreDetail)
    {
        if (!excitmentScoreDetail.Any())
            return "This match has standard excitement potential.";

        var excitement = DetermineExcitementLevel(excitmentScoreDetail.Average(f => f.Value.value));
        var factors = excitmentScoreDetail.OrderByDescending(c => c.Value).Select(f => MatchScoreUtil.HighValuePhrases[f.Key]).ToList();

        return factors.Count switch
        {
            1 => $"This match has {excitement} excitement potential due to {factors[0]}.",
            2 => $"This match has {excitement} excitement potential due to {factors[0]} and {factors[1]}.",
            _ => $"This match has {excitement} excitement potential due to {factors[0]}, {factors[1]}, and {factors[2]}."
        };
    }

    private static string DetermineExcitementLevel(double averageScore)
    {
        return averageScore switch
        {
            >= 0.8 => "exceptional",
            >= 0.6 => "high",
            >= 0.4 => "moderate",
            >= 0.2 => "modest",
            _ => "low"
        };
    }
}
