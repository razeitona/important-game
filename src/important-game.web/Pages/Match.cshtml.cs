using important_game.infrastructure.Contexts.Matches;
using important_game.infrastructure.Contexts.Matches.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class MatchModel(ILogger<MatchModel> _logger, IMatchesService _matchService) : PageModel
    {
        public MatchDetailDto MatchInfo { get; private set; }


        public async Task OnGet([FromRoute] int id)
        {
            var match = await _matchService.GetMatchByIdAsync(id);

            if (match == null)
                return;

            MatchInfo = match;

            //MatchInfo = new ExcitmentMatchDetailResponse
            //{
            //    AwayTeam = match.AwayTeam,
            //    ExcitementScore = match.ExcitementScore,
            //    Headtohead = match.Headtohead,
            //    HomeTeam = match.HomeTeam,
            //    Id = match.Id,
            //    League = match.League,
            //    MatchDate = match.MatchDate,
            //    ExcitmentScoreDetail = match.ExcitmentScoreDetail,
            //    IsLive = match.IsLive,
            //    IsRivalry = match.ExcitmentScoreDetail.Any(c => c.Key == "Rivalry" && c.Value.Value > 0d),
            //    HasTitleHolder = match.HomeTeam.IsTitleHolder || match.AwayTeam.IsTitleHolder,
            //    Description = match.Description,
            //    LiveExcitementScore = match.LiveExcitementScore
            //    //Score = match.Score,
            //};

        }

    }
}
