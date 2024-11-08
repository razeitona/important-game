using important_game.infrastructure.ImportantMatch;
using important_game.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class MatchModel(ILogger<MatchModel> _logger, IExcitmentMatchService _matchService) : PageModel
    {
        public ExcitmentMatchDetailResponse MatchInfo { get; private set; }


        public void OnGet([FromRoute] int id)
        {
            var match = _matchService.GetMatchByIdAsync(id);

            if (match == null)
                return;

            MatchInfo = new ExcitmentMatchDetailResponse
            {
                AwayTeam = match.AwayTeam,
                ExcitementScore = match.ExcitementScore,
                Headtohead = match.Headtohead,
                HomeTeam = match.HomeTeam,
                Id = match.Id,
                League = match.League,
                MatchDate = match.MatchDate,
                ExcitmentScoreDetail = match.ExcitmentScoreDetail,
                IsLive = match.IsLive,
                //Score = match.Score,
            };

        }

    }
}
