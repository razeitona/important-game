﻿namespace important_game.infrastructure.ImportantMatch.Models
{
    public class ExcitementMatchDto
    {
        public int Id { get; init; }
        public LeagueDto League { get; init; }
        public DateTimeOffset MatchDate { get; init; }
        public TeamDto HomeTeam { get; init; }
        public TeamDto AwayTeam { get; init; }
        public double ExcitementScore { get; init; }
        public double LiveExcitementScore { get; init; }
        public bool IsLive { get; set; }
        public bool IsTopTrend { get; set; }
        public int Favorites { get; set; }

        public double Score => IsLive ? this.LiveExcitementScore : this.ExcitementScore;
    }
}