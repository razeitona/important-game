using important_game.infrastructure.ImportantMatch.Data;
using important_game.infrastructure.ImportantMatch.Data.Entities;
using important_game.infrastructure.ImportantMatch.Live;
using important_game.infrastructure.ImportantMatch.Models;

namespace important_game.infrastructure.ImportantMatch
{
    public class ExcitmentMatchService(IExctimentMatchRepository matchRepository
        , IExcitmentMatchProcessor matchProcessor, IExcitmentMatchLiveProcessor liveProcessor) : IExcitmentMatchService
    {

        public async Task<List<ExcitementMatchDto>> GetAllMatchesAsync()
        {
            var rawMatches = await matchRepository.GetAllMatchesAsync();
            var matches = new List<ExcitementMatchDto>();

            foreach (var rawMatch in rawMatches.Where(m => m.MatchDate > DateTime.UtcNow.AddMinutes(-110)))
            {
                var match = new ExcitementMatchDto
                {
                    Id = rawMatch.Id,
                    MatchDate = rawMatch.MatchDate,
                    League = new LeagueDto
                    {
                        Id = rawMatch.League.Id,
                        Name = rawMatch.League.Name,
                        BackgroundColor = rawMatch.League.BackgroundColor
                    },
                    IsLive = rawMatch.MatchDate < DateTime.UtcNow,
                    ExcitementScore = rawMatch.ExcitementScore,
                    LiveExcitementScore = rawMatch.LiveExcitementScore,
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

                matches.Add(match);

            }

            return matches.OrderBy(c => c.MatchDate).ToList();
        }

        public async Task<ExcitementMatchDetailDto> GetMatchByIdAsync(int id)
        {
            var rawMatch = await matchRepository.GetMatchByIdAsync(id);

            if (rawMatch == null)
                return null;

            var match = new ExcitementMatchDetailDto
            {
                Id = rawMatch.Id,
                MatchDate = rawMatch.MatchDate,
                League = new LeagueDto
                {
                    Id = rawMatch.League.Id,
                    Name = rawMatch.League.Name,
                    BackgroundColor = rawMatch.League.BackgroundColor
                },
                IsLive = rawMatch.MatchDate < DateTime.UtcNow,
                ExcitementScore = rawMatch.ExcitementScore,
                LiveExcitementScore = rawMatch.LiveExcitementScore,
                HomeTeam = SetupMatchDetailTeam(rawMatch.HomeTeam),
                AwayTeam = SetupMatchDetailTeam(rawMatch.AwayTeam),
                Headtohead = SetupMatchHeadToHead(rawMatch.HeadToHead),
                ExcitmentScoreDetail = rawMatch.Score,
            };


            return match;
        }

        private TeamMatchDetailDto SetupMatchDetailTeam(Team team)
        {
            var matchTeam = new TeamMatchDetailDto()
            {
                Id = team.Id,
                Name = team.Name,
                IsTitleHolder = team.IsTitleHolder,
                Form = team.LastFixtures.FixtureResult
            };

            return matchTeam;
        }

        private List<FixtureDto> SetupMatchHeadToHead(List<Fixture> headToHead)
        {
            var matches = new List<FixtureDto>();

            foreach (var fixture in headToHead)
            {
                matches.Add(new FixtureDto
                {
                    MatchDate = fixture.MatchDate,
                    HomeTeamName = fixture.HomeTeam.Name,
                    AwayTeamName = fixture.AwayTeam.Name,
                    HomeTeamScore = fixture.HomeTeamScore,
                    AwayTeamScore = fixture.AwayTeamScore,
                });

            }

            return matches;
        }

        public async Task CalculateUpcomingMatchsExcitment()
        {
            var excitementMatches = await matchProcessor.GetUpcomingExcitementMatchesAsync(new ExctimentMatchOptions());

            var currentMatches = await GetAllMatchesAsync();

            var validMatches = excitementMatches.Select(c => c.Id).ToHashSet();

            var liveGames = currentMatches
                  .Where(c => c.MatchDate < DateTime.UtcNow && c.MatchDate > DateTime.UtcNow.AddMinutes(-110))
                  .Where(c => !validMatches.Contains(c.Id))
                  .ToList();

            //excitementMatches.AddRange(liveGames);

            await matchRepository.SaveMatchesAsync(excitementMatches);
        }

    }
}
