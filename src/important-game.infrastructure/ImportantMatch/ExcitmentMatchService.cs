using important_game.infrastructure.Shared.Extensions;
using important_game.infrastructure.Data.Repositories;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using important_game.infrastructure.Contexts.Competitions.Data.Entities;
using important_game.infrastructure.ImportantMatch.Live;
using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.infrastructure.ImportantMatch
{
    public class ExcitmentMatchService(
        IMatchRepository matchRepository,
        ILiveMatchRepository liveMatchRepository,
        IExcitmentMatchProcessor matchProcessor,
        IExcitmentMatchLiveProcessor liveProcessor) : IExcitmentMatchService
    {
        public async Task CalculateUpcomingMatchsExcitment()
        {
            await matchProcessor.CalculateUpcomingMatchsExcitment();
        }

        public async Task CalculateUnfinishedMatchExcitment()
        {
            var listOfLiveMatches = await matchRepository.GetUnfinishedMatchesAsync();

            foreach (var match in listOfLiveMatches)
            {
                if (match.MatchDateUTC > DateTime.UtcNow)
                    continue;

                var liveMatch = await liveProcessor.ProcessLiveMatchData(match);

                await matchRepository.SaveMatchAsync(match);
                if (liveMatch == null)
                    continue;

                await liveMatchRepository.SaveLiveMatchAsync(liveMatch);
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
                HomeTeam = SetupMatchDetailTeam(rawMatch.HomeTeam, rawMatch.HomeForm, rawMatch.HomeTeamPosition, rawMatch.Competition),
                AwayTeam = SetupMatchDetailTeam(rawMatch.AwayTeam, rawMatch.AwayForm, rawMatch.AwayTeamPosition, rawMatch.Competition),
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
            { "League Coeficient", "significant impact on league standings" },
            { "League Standings", "high table impact" },
            { "Fixture Importance", "crucial stage in the season" },
            { "Teams Form", "teams recent performances" },
            { "Teams Goals", "high scoring potential" },
            { "Head to head", "compelling head-to-head history" },
            { "Rivalry", "historical rivalry between the teams" },
            { "Title Holder", "presence of the defending champions" },
            { "Score Line", "tight match" },
            { "xGoals", "high value of xGoals" },
            { "Fouls", "few stoppages" },
            { "Cards", "cards with high impact in the final result" },
            { "Possession", "teams fighting for the win" },
            { "Big chances", "amount of big chances" },
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

        private string BuildSentence(Dictionary<string, (bool show, double value)> excitmentScoreDetail)
        {
            if (!excitmentScoreDetail.Any())
                return "This match has standard excitement potential.";

            var excitement = DetermineExcitementLevel(excitmentScoreDetail.Average(f => f.Value.value));
            var factors = excitmentScoreDetail.OrderByDescending(c => c.Value).Select(f => _highValuePhrases[f.Key]).ToList();

            return factors.Count switch
            {
                1 => $"This match has {excitement} excitement potential due to {factors[0]}.",
                2 => $"This match has {excitement} excitement potential due to {factors[0]} and {factors[1]}.",
                _ => $"This match has {excitement} excitement potential due to {factors[0]}, {factors[1]}, and {factors[2]}."
            };
        }

        private Dictionary<string, (bool Show, double Value)> SetupExcitmentScoreDetail(Match rawMatch, LiveMatch? liveData)
        {
            if (liveData != null)
            {
                return new Dictionary<string, (bool Show, double Value)>
                {
                    { "Score Line", (true,liveData.ScoreLineScore) },
                    { "xGoals", (true,liveData.XGoalsScore) },
                    { "Fouls", (true,liveData.TotalFoulsScore) },
                    { "Cards", (true,liveData.TotalCardsScore) },
                    { "Possession", (true,liveData.PossesionScore) },
                    { "Big chances", (true, liveData.BigChancesScore) },
                };
            }

            return new Dictionary<string, (bool Show, double Value)>
            {
                { "League Coeficient", (true,rawMatch.CompetitionScore) },
                { "League Standings", (true, rawMatch.CompetitionStandingScore) },
                { "Fixture Importance", (true, rawMatch.FixtureScore) },
                { "Teams Form", (true, rawMatch.FormScore) },
                { "Teams Goals", (true, rawMatch.GoalsScore) },
                { "Head to head", (true, rawMatch.HeadToHeadScore) },
                { "Rivalry", (false, rawMatch.RivalryScore) },
                { "Title Holder", (false, rawMatch.TitleHolderScore) },
            };
        }

        private TeamMatchDetailDto SetupMatchDetailTeam(Team team, string? teamForm, int teamPosition, Competition competition)
        {
            var form = teamForm?.Split(",").ToList() ?? new();

            var matchTeam = new TeamMatchDetailDto()
            {
                Id = team.Id,
                Name = team.Name,
                IsTitleHolder = team.Id == competition.TitleHolderTeamId,
                TablePosition = teamPosition
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
