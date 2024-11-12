using important_game.infrastructure.Extensions;
using important_game.infrastructure.ImportantMatch.Data;
using important_game.infrastructure.ImportantMatch.Data.Entities;
using important_game.infrastructure.ImportantMatch.Live;
using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.infrastructure.ImportantMatch
{
    public class ExcitmentMatchService(IExctimentMatchRepository matchRepository
        , IExcitmentMatchProcessor matchProcessor, IExcitmentMatchLiveProcessor liveProcessor) : IExcitmentMatchService
    {
        public async Task CalculateUpcomingMatchsExcitment()
        {
            await matchProcessor.CalculateUpcomingMatchsExcitment();
        }

        public async Task CalculateUnfinishedMatchExcitment()
        {
            var listOfLiveMatches = await matchRepository.GetUnfinishedMatchesAsync();

            //Process all the leagues to identify the excitement match rating for each
            foreach (var match in listOfLiveMatches)
            {
                //Get all upcoming games (active) from processing competition
                var liveMatch = await liveProcessor.ProcessLiveMatchData(match);

                if (liveMatch == null)
                    continue;

                await matchRepository.SaveMatchAsync(match);
                await matchRepository.SaveLiveMatchAsync(liveMatch);
            }

        }

        public async Task<List<ExcitementMatchDto>> GetAllMatchesAsync()
        {
            var rawMatches = await matchRepository.GetUpcomingMatchesAsync();

            var matches = new List<ExcitementMatchDto>();

            foreach (var rawMatch in rawMatches)
            {
                var match = new ExcitementMatchDto
                {
                    Id = rawMatch.Id,
                    MatchDate = rawMatch.MatchDateUTC,
                    League = new LeagueDto
                    {
                        Id = rawMatch.Competition.Id,
                        Name = rawMatch.Competition.Name,
                        BackgroundColor = rawMatch.Competition.BackgroundColor
                    },
                    IsLive = rawMatch.MatchStatus == MatchStatus.Live,
                    ExcitementScore = rawMatch.ExcitmentScore,
                    LiveExcitementScore = rawMatch.LiveMatches?.LastOrDefault()?.ExcitmentScore ?? rawMatch.ExcitmentScore,
                    HomeTeam = new TeamDto
                    {
                        Id = rawMatch.HomeTeam.Id,
                        Name = rawMatch.HomeTeam.Name,
                        Slug = SlugHelper.GenerateSlug(rawMatch.HomeTeam.Name),
                    },
                    AwayTeam = new TeamDto
                    {
                        Name = rawMatch.AwayTeam.Name,
                        Id = rawMatch.AwayTeam.Id,
                        Slug = SlugHelper.GenerateSlug(rawMatch.AwayTeam.Name),
                    },
                };

                matches.Add(match);

            }

            return matches.OrderBy(c => c.MatchDate).ToList();
        }

        public async Task<List<ExcitementMatchLiveDto>> GetLiveMatchesAsync()
        {
            var rawMatches = await matchRepository.GetUnfinishedMatchesAsync();

            var matches = new List<ExcitementMatchLiveDto>();

            foreach (var rawMatch in rawMatches)
            {

                var match = new ExcitementMatchLiveDto
                {
                    Id = rawMatch.Id,
                    MatchDate = rawMatch.MatchDateUTC,
                    League = new LeagueDto
                    {
                        Id = rawMatch.Competition.Id,
                        Name = rawMatch.Competition.Name,
                        BackgroundColor = rawMatch.Competition.BackgroundColor
                    },
                    IsLive = true,
                    ExcitementScore = rawMatch.ExcitmentScore,
                    HomeTeam = new TeamDto
                    {
                        Id = rawMatch.HomeTeam.Id,
                        Name = rawMatch.HomeTeam.Name,
                    },
                    AwayTeam = new TeamDto
                    {
                        Name = rawMatch.AwayTeam.Name,
                        Id = rawMatch.AwayTeam.Id,
                    }
                };

                var liveMatchInfo = rawMatch.LiveMatches.LastOrDefault();
                if (liveMatchInfo != null)
                {
                    match.LiveExcitementScore = liveMatchInfo.ExcitmentScore;
                    match.Minutes = liveMatchInfo.Minutes;
                }

                matches.Add(match);

            }

            return matches.OrderBy(c => c.MatchDate).ToList();
        }


        public async Task<ExcitementMatchDetailDto> GetMatchByIdAsync(int id)
        {
            var rawMatch = await matchRepository.GetMatchByIdAsync(id);

            if (rawMatch == null)
                return null;

            var liveData = rawMatch.LiveMatches.LastOrDefault();



            var match = new ExcitementMatchDetailDto
            {
                Id = rawMatch.Id,
                MatchDate = rawMatch.MatchDateUTC,
                League = new LeagueDto
                {
                    Id = rawMatch.Competition.Id,
                    Name = rawMatch.Competition.Name,
                    BackgroundColor = rawMatch.Competition.BackgroundColor
                },
                IsLive = rawMatch.MatchStatus == MatchStatus.Live,
                ExcitementScore = rawMatch.ExcitmentScore,
                HomeTeam = SetupMatchDetailTeam(rawMatch.HomeTeam, rawMatch.HomeForm, rawMatch.Competition),
                AwayTeam = SetupMatchDetailTeam(rawMatch.AwayTeam, rawMatch.AwayForm, rawMatch.Competition),
                Headtohead = SetupMatchHeadToHead(rawMatch.HeadToHead.ToList()),
                ExcitmentScoreDetail = SetupExcitmentScoreDetail(rawMatch, liveData)
            };

            match.Description = BuildSentence(match.ExcitmentScoreDetail);

            if (liveData != null)
            {
                match.LiveExcitementScore = liveData.ExcitmentScore;
                match.Minutes = liveData.Minutes;
            }

            return match;
        }

        private readonly Dictionary<string, string> _highValuePhrases = new()
        {
            { "CompetitionScore", "significant impact on league standings" },
            { "FixtureScore", "crucial stage in the season" },
            { "FormScore", "both teams' impressive recent performances" },
            { "CompetitionStandingScore", "high table impact" },
            { "GoalsScore", "high scoring potential" },
            { "HeadToHeadScore", "compelling head-to-head history" },
            { "RivalryScore", "historical rivalry between the teams" },
            { "TitleHolderScore", "presence of the defending champions" }
        };


        private string DetermineExcitementLevel(double averageScore)
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

        private string BuildSentence(Dictionary<string, double> excitmentScoreDetail)
        {
            if (!excitmentScoreDetail.Any())
                return "This match has standard excitement potential.";

            var excitement = DetermineExcitementLevel(excitmentScoreDetail.Average(f => f.Value));
            var factors = excitmentScoreDetail.OrderByDescending(c => c.Value).Select(f => _highValuePhrases[f.Key]).ToList();

            return factors.Count switch
            {
                1 => $"This match has {excitement} excitement potential due to {factors[0]}.",
                2 => $"This match has {excitement} excitement potential due to {factors[0]} and {factors[1]}.",
                _ => $"This match has {excitement} excitement potential due to {factors[0]}, {factors[1]}, and {factors[2]}."
            };
        }

        private Dictionary<string, double> SetupExcitmentScoreDetail(Match rawMatch, LiveMatch? liveData)
        {

            if (liveData != null)
            {
                return new Dictionary<string, double>
                {
                    { "CompetitionScore", liveData.ScoreLineScore },
                    { "FixtureScore", liveData.ShotTargetScore },
                    { "FormScore", liveData.XGoalsScore },
                    { "GoalsScore", liveData.TotalFoulsScore },
                    { "CompetitionStandingScore", liveData.TotalCardsScore },
                    { "HeadToHeadScore", liveData.PossesionScore },
                    { "RivalryScore", liveData.BigChancesScore },
                };
            }

            return new Dictionary<string, double>
            {
                { "CompetitionScore", rawMatch.CompetitionScore },
                { "FixtureScore", rawMatch.FixtureScore },
                { "FormScore", rawMatch.FormScore },
                { "GoalsScore", rawMatch.GoalsScore },
                { "CompetitionStandingScore", rawMatch.CompetitionStandingScore },
                { "HeadToHeadScore", rawMatch.HeadToHeadScore },
                { "RivalryScore", rawMatch.RivalryScore },
                { "TitleHolderScore", rawMatch.TitleHolderScore },
            };
        }

        private TeamMatchDetailDto SetupMatchDetailTeam(Team team, string? teamForm, Competition competition)
        {
            var form = teamForm?.Split(",").ToList() ?? new();

            var matchTeam = new TeamMatchDetailDto()
            {
                Id = team.Id,
                Name = team.Name,
                IsTitleHolder = team.Id == competition.TitleHolderTeamId
            };

            if (form.Count > 0)
            {
                matchTeam.Form = form.Select(c => c switch
                {
                    "1" => MatchResultType.Win,
                    "0" => MatchResultType.Draw,
                    "-1" => MatchResultType.Lost,
                }).ToList();
            }

            return matchTeam;
        }

        private List<FixtureDto> SetupMatchHeadToHead(List<Headtohead> headToHead)
        {
            var matches = new List<FixtureDto>();

            foreach (var fixture in headToHead)
            {
                matches.Add(new FixtureDto
                {
                    MatchDate = fixture.MatchDateUTC,
                    HomeTeamName = fixture.HomeTeam.Name,
                    AwayTeamName = fixture.AwayTeam.Name,
                    HomeTeamScore = fixture.HomeTeamScore,
                    AwayTeamScore = fixture.AwayTeamScore,
                });

            }

            return matches;
        }

    }
}
