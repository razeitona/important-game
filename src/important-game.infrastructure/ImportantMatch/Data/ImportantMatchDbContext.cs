using important_game.infrastructure.ImportantMatch.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace important_game.infrastructure.ImportantMatch.Data
{
    // Define the DbContext
    public class ImportantMatchDbContext : DbContext
    {
        public DbSet<Team> Teams { get; set; }
        public DbSet<Competition> Competitions { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<LiveMatch> LiveMatches { get; set; }
        public DbSet<Headtohead> HeadtoheadMatches { get; set; }
        public DbSet<Rivalry> Rivalries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configure the connection to the SQLite database
            optionsBuilder.UseSqlite("Data Source=matchwatch.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MatchConfiguration());

            SetupCompetitionDependencies(modelBuilder);
            SetupMatchDependencies(modelBuilder);
            SetupLiveMatchDependencies(modelBuilder);
            SetupRivalryDependencies(modelBuilder);
            SetupHeadToheadDependencies(modelBuilder);

            InsertCompetitionDefaultData(modelBuilder);
            InsertRivalryDefaultData(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }


        private void SetupCompetitionDependencies(ModelBuilder modelBuilder)
        {
            //// Configure the entity model
            //modelBuilder.Entity<Competition>()
            //    .HasOne(c => c.TitleHolderTeam)
            //    .WithMany()
            //    .HasForeignKey(c => c.TitleHolderTeamId)
            //    .OnDelete(DeleteBehavior.SetNull);
        }

        private void SetupMatchDependencies(ModelBuilder modelBuilder)
        {
            // Configure the entity model
            modelBuilder.Entity<Match>()
              .HasOne(f => f.Competition)
              .WithMany(c => c.Fixtures)
              .HasForeignKey(f => f.CompetitionId)
              .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Match>()
                .HasOne(f => f.HomeTeam)
                .WithMany(t => t.HomeFixtures)
                .HasForeignKey(f => f.HomeTeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Match>()
                .HasOne(f => f.AwayTeam)
                .WithMany(t => t.AwayFixtures)
                .HasForeignKey(f => f.AwayTeamId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void SetupLiveMatchDependencies(ModelBuilder modelBuilder)
        {
            // Configure the entity model
            modelBuilder.Entity<LiveMatch>()
                .HasOne(f => f.Match)
                .WithMany(t => t.LiveMatches)
                .HasForeignKey(f => f.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void SetupRivalryDependencies(ModelBuilder modelBuilder)
        {
            // Configure the entity model
            //modelBuilder.Entity<Rivalry>()
            //    .HasOne(f => f.TeamOne)
            //    .WithMany(t => t.TeamOneRivalries)
            //    .HasForeignKey(f => f.TeamOneId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Rivalry>()
            //    .HasOne(f => f.TeamTwo)
            //    .WithMany(t => t.TeamTwoRivalries)
            //    .HasForeignKey(f => f.TeamTwoId)
            //    .OnDelete(DeleteBehavior.Cascade);
        }

        private void SetupHeadToheadDependencies(ModelBuilder modelBuilder)
        {
            // Configure the entity model
            modelBuilder.Entity<Headtohead>()
              .HasOne(f => f.Match)
              .WithMany(c => c.HeadToHead)
              .HasForeignKey(f => f.MatchId)
              .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Headtohead>()
                .HasOne(f => f.HomeTeam)
                .WithMany(t => t.HomeHeadToHead)
                .HasForeignKey(f => f.HomeTeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Headtohead>()
                .HasOne(f => f.AwayTeam)
                .WithMany(t => t.AwayHeadToHead)
                .HasForeignKey(f => f.AwayTeamId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void InsertCompetitionDefaultData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Competition>().HasData(
                new Competition { Id = 7, Name = "Champions League", LeagueRanking = 1d, PrimaryColor = "#3c1c5a", BackgroundColor = "#ffffff", IsActive = true },
                new Competition { Id = 17, Name = "Premier League", LeagueRanking = 0.95d, PrimaryColor = "#3c1c5a", BackgroundColor = "#3d195b", IsActive = true },
                new Competition { Id = 35, Name = "Bundesliga", LeagueRanking = 0.85d, PrimaryColor = "#ffffff", BackgroundColor = "#e2080e", IsActive = true },
                new Competition { Id = 8, Name = "La Liga", LeagueRanking = 0.85d, PrimaryColor = "#ffffff", BackgroundColor = "#2f4a89", IsActive = true },
                new Competition { Id = 23, Name = "Serie A", LeagueRanking = 0.82d, PrimaryColor = "#ffffff", BackgroundColor = "#09519e", IsActive = true },
                new Competition { Id = 34, Name = "Ligue 1", LeagueRanking = 0.75d, PrimaryColor = "#3c1c5a", BackgroundColor = "#ffffff", IsActive = true },
                new Competition { Id = 37, Name = "Eredevisie", LeagueRanking = 0.7d, PrimaryColor = "#122e62", BackgroundColor = "#122e62", IsActive = true },
                new Competition { Id = 325, Name = "Brasileirão", LeagueRanking = 0.7d, PrimaryColor = "#141528", BackgroundColor = "#C7FF00", IsActive = true },
                new Competition { Id = 155, Name = "Argentina", LeagueRanking = 0.65d, PrimaryColor = "#004a79", BackgroundColor = "#33c5df", IsActive = true },
                new Competition { Id = 52, Name = "Turkey", LeagueRanking = 0.6d, PrimaryColor = "#f00515", BackgroundColor = "#f00918", IsActive = true },
                new Competition { Id = 238, Name = "Liga Portugal", LeagueRanking = 0.6d, PrimaryColor = "#001841", BackgroundColor = "#ffc501", IsActive = true },
                new Competition { Id = 18, Name = "Championship", LeagueRanking = 0.50d, PrimaryColor = "#3c1c5a", BackgroundColor = "#ffffff", IsActive = true },
                new Competition { Id = 36, Name = "Scotland", LeagueRanking = 0.55d, PrimaryColor = "#311b77", BackgroundColor = "#ffffff", IsActive = true },
                new Competition { Id = 955, Name = "Saudi League", LeagueRanking = 0.4d, PrimaryColor = "#ffffff", BackgroundColor = "#2c9146", IsActive = true },
                new Competition { Id = 11621, Name = "Liga MX", LeagueRanking = 0.65d, PrimaryColor = "#3c1c5a", BackgroundColor = "#ffffff", IsActive = true },
                new Competition { Id = 185, Name = "Greek League", LeagueRanking = 0.55d, PrimaryColor = "#3c1c5a", BackgroundColor = "#ffffff", IsActive = true }
            );
        }

        private void InsertRivalryDefaultData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rivalry>().HasData(
                new Rivalry { Id = 1, TeamOneId = 42, TeamTwoId = 2672, RivarlyValue = 0.7 },
                new Rivalry { Id = 2, TeamOneId = 2817, TeamTwoId = 2672, RivarlyValue = 0.8 },
                new Rivalry { Id = 3, TeamOneId = 38, TeamTwoId = 2817, RivarlyValue = 0.85 },
                new Rivalry { Id = 4, TeamOneId = 38, TeamTwoId = 2829, RivarlyValue = 0.9 },
                new Rivalry { Id = 5, TeamOneId = 2218, TeamTwoId = 2404, RivarlyValue = 0.75 },
                new Rivalry { Id = 6, TeamOneId = 2032, TeamTwoId = 5149, RivarlyValue = 0.9 },
                new Rivalry { Id = 7, TeamOneId = 17, TeamTwoId = 2829, RivarlyValue = 0.8 },
                new Rivalry { Id = 8, TeamOneId = 17, TeamTwoId = 2672, RivarlyValue = 0.75 },
                new Rivalry { Id = 9, TeamOneId = 1644, TeamTwoId = 2817, RivarlyValue = 0.85 },
                new Rivalry { Id = 10, TeamOneId = 2672, TeamTwoId = 2829, RivarlyValue = 0.95 },
                new Rivalry { Id = 11, TeamOneId = 2673, TeamTwoId = 2829, RivarlyValue = 0.8 },
                new Rivalry { Id = 12, TeamOneId = 2687, TeamTwoId = 2829, RivarlyValue = 0.9 },
                new Rivalry { Id = 13, TeamOneId = 44, TeamTwoId = 2829, RivarlyValue = 0.95 },
                new Rivalry { Id = 14, TeamOneId = 44, TeamTwoId = 2687, RivarlyValue = 0.85 },
                new Rivalry { Id = 15, TeamOneId = 2697, TeamTwoId = 2817, RivarlyValue = 0.9 },
                new Rivalry { Id = 16, TeamOneId = 3006, TeamTwoId = 3002, RivarlyValue = 0.95 },
                new Rivalry { Id = 17, TeamOneId = 3006, TeamTwoId = 3001, RivarlyValue = 1.0 },
                new Rivalry { Id = 18, TeamOneId = 3002, TeamTwoId = 3001, RivarlyValue = 0.85 },
                new Rivalry { Id = 19, TeamOneId = 2995, TeamTwoId = 3002, RivarlyValue = 0.6 },
                new Rivalry { Id = 20, TeamOneId = 3009, TeamTwoId = 2999, RivarlyValue = 0.75 },
                new Rivalry { Id = 21, TeamOneId = 1648, TeamTwoId = 1643, RivarlyValue = 0.7 },
                new Rivalry { Id = 22, TeamOneId = 1647, TeamTwoId = 1684, RivarlyValue = 0.6 },
                new Rivalry { Id = 23, TeamOneId = 1649, TeamTwoId = 1678, RivarlyValue = 0.85 },
                new Rivalry { Id = 24, TeamOneId = 1653, TeamTwoId = 1661, RivarlyValue = 0.7 },
                new Rivalry { Id = 25, TeamOneId = 1641, TeamTwoId = 1644, RivarlyValue = 0.9 },
                new Rivalry { Id = 26, TeamOneId = 1649, TeamTwoId = 1641, RivarlyValue = 0.8 },
                new Rivalry { Id = 27, TeamOneId = 1641, TeamTwoId = 1678, RivarlyValue = 0.75 },
                new Rivalry { Id = 28, TeamOneId = 1647, TeamTwoId = 1678, RivarlyValue = 0.7 },
                new Rivalry { Id = 29, TeamOneId = 2672, TeamTwoId = 2677, RivarlyValue = 0.6 },
                new Rivalry { Id = 30, TeamOneId = 2672, TeamTwoId = 2673, RivarlyValue = 0.95 },
                new Rivalry { Id = 31, TeamOneId = 2672, TeamTwoId = 2534, RivarlyValue = 0.7 },
                new Rivalry { Id = 32, TeamOneId = 2524, TeamTwoId = 2534, RivarlyValue = 0.6 },
                new Rivalry { Id = 33, TeamOneId = 2538, TeamTwoId = 2677, RivarlyValue = 0.55 },
                new Rivalry { Id = 34, TeamOneId = 2697, TeamTwoId = 2692, RivarlyValue = 0.95 },
                new Rivalry { Id = 35, TeamOneId = 2699, TeamTwoId = 2702, RivarlyValue = 0.95 },
                new Rivalry { Id = 36, TeamOneId = 2687, TeamTwoId = 2696, RivarlyValue = 0.85 },
                new Rivalry { Id = 37, TeamOneId = 2697, TeamTwoId = 2687, RivarlyValue = 0.9 },
                new Rivalry { Id = 38, TeamOneId = 2714, TeamTwoId = 2702, RivarlyValue = 0.85 },
                new Rivalry { Id = 39, TeamOneId = 2685, TeamTwoId = 2693, RivarlyValue = 0.65 },
                new Rivalry { Id = 40, TeamOneId = 2685, TeamTwoId = 2690, RivarlyValue = 0.6 },
                new Rivalry { Id = 41, TeamOneId = 2693, TeamTwoId = 2687, RivarlyValue = 0.85 },
                new Rivalry { Id = 42, TeamOneId = 2687, TeamTwoId = 2692, RivarlyValue = 0.9 },
                new Rivalry { Id = 43, TeamOneId = 2687, TeamTwoId = 2714, RivarlyValue = 0.85 },
                new Rivalry { Id = 44, TeamOneId = 2687, TeamTwoId = 2702, RivarlyValue = 0.85 },
                new Rivalry { Id = 45, TeamOneId = 2693, TeamTwoId = 2705, RivarlyValue = 0.6 },
                new Rivalry { Id = 46, TeamOneId = 2693, TeamTwoId = 2699, RivarlyValue = 0.7 },
                new Rivalry { Id = 47, TeamOneId = 2693, TeamTwoId = 2702, RivarlyValue = 0.75 },
                new Rivalry { Id = 48, TeamOneId = 2953, TeamTwoId = 2959, RivarlyValue = 0.9 },
                new Rivalry { Id = 49, TeamOneId = 2953, TeamTwoId = 2952, RivarlyValue = 0.85 },
                new Rivalry { Id = 50, TeamOneId = 2952, TeamTwoId = 2959, RivarlyValue = 0.8 },
                new Rivalry { Id = 51, TeamOneId = 2953, TeamTwoId = 2950, RivarlyValue = 0.65 },
                new Rivalry { Id = 52, TeamOneId = 2352, TeamTwoId = 2351, RivarlyValue = 1.0 },
                new Rivalry { Id = 53, TeamOneId = 2829, TeamTwoId = 2817, RivarlyValue = 1.0 },
                new Rivalry { Id = 54, TeamOneId = 2829, TeamTwoId = 2825, RivarlyValue = 0.8 },
                new Rivalry { Id = 55, TeamOneId = 2817, TeamTwoId = 2836, RivarlyValue = 0.85 },
                new Rivalry { Id = 56, TeamOneId = 2829, TeamTwoId = 2836, RivarlyValue = 0.9 },
                new Rivalry { Id = 57, TeamOneId = 2836, TeamTwoId = 2825, RivarlyValue = 0.75 },
                new Rivalry { Id = 58, TeamOneId = 2836, TeamTwoId = 2833, RivarlyValue = 0.8 },
                new Rivalry { Id = 59, TeamOneId = 2825, TeamTwoId = 2824, RivarlyValue = 0.85 },
                new Rivalry { Id = 60, TeamOneId = 2828, TeamTwoId = 2819, RivarlyValue = 0.75 },
                new Rivalry { Id = 61, TeamOneId = 2817, TeamTwoId = 2814, RivarlyValue = 0.85 },
                new Rivalry { Id = 62, TeamOneId = 2816, TeamTwoId = 2833, RivarlyValue = 0.9 },
                new Rivalry { Id = 63, TeamOneId = 2859, TeamTwoId = 2845, RivarlyValue = 0.6 },
                new Rivalry { Id = 64, TeamOneId = 17, TeamTwoId = 42, RivarlyValue = 0.85 },
                new Rivalry { Id = 65, TeamOneId = 17, TeamTwoId = 44, RivarlyValue = 0.9 },
                new Rivalry { Id = 66, TeamOneId = 17, TeamTwoId = 38, RivarlyValue = 0.85 },
                new Rivalry { Id = 67, TeamOneId = 17, TeamTwoId = 35, RivarlyValue = 0.9 },
                new Rivalry { Id = 68, TeamOneId = 17, TeamTwoId = 33, RivarlyValue = 0.8 },
                new Rivalry { Id = 69, TeamOneId = 42, TeamTwoId = 44, RivarlyValue = 0.9 },
                new Rivalry { Id = 70, TeamOneId = 42, TeamTwoId = 40, RivarlyValue = 0.7 },
                new Rivalry { Id = 71, TeamOneId = 42, TeamTwoId = 38, RivarlyValue = 0.9 },
                new Rivalry { Id = 72, TeamOneId = 42, TeamTwoId = 35, RivarlyValue = 0.95 },
                new Rivalry { Id = 73, TeamOneId = 42, TeamTwoId = 33, RivarlyValue = 1.0 },
                new Rivalry { Id = 74, TeamOneId = 42, TeamTwoId = 37, RivarlyValue = 0.8 },
                new Rivalry { Id = 75, TeamOneId = 39, TeamTwoId = 35, RivarlyValue = 0.75 },
                new Rivalry { Id = 76, TeamOneId = 44, TeamTwoId = 38, RivarlyValue = 0.85 },
                new Rivalry { Id = 77, TeamOneId = 44, TeamTwoId = 35, RivarlyValue = 1.0 },
                new Rivalry { Id = 78, TeamOneId = 44, TeamTwoId = 48, RivarlyValue = 0.9 },
                new Rivalry { Id = 79, TeamOneId = 40, TeamTwoId = 38, RivarlyValue = 0.75 },
                new Rivalry { Id = 80, TeamOneId = 40, TeamTwoId = 37, RivarlyValue = 0.65 },
                new Rivalry { Id = 81, TeamOneId = 40, TeamTwoId = 3, RivarlyValue = 0.7 },
                new Rivalry { Id = 82, TeamOneId = 30, TeamTwoId = 7, RivarlyValue = 0.7 },
                new Rivalry { Id = 83, TeamOneId = 14, TeamTwoId = 38, RivarlyValue = 0.65 },
                new Rivalry { Id = 84, TeamOneId = 14, TeamTwoId = 31, RivarlyValue = 0.7 },
                new Rivalry { Id = 85, TeamOneId = 38, TeamTwoId = 50, RivarlyValue = 0.65 },
                new Rivalry { Id = 86, TeamOneId = 38, TeamTwoId = 35, RivarlyValue = 0.9 },
                new Rivalry { Id = 87, TeamOneId = 38, TeamTwoId = 43, RivarlyValue = 0.7 },
                new Rivalry { Id = 88, TeamOneId = 38, TeamTwoId = 33, RivarlyValue = 0.95 },
                new Rivalry { Id = 89, TeamOneId = 38, TeamTwoId = 37, RivarlyValue = 0.85 },
                new Rivalry { Id = 90, TeamOneId = 38, TeamTwoId = 7, RivarlyValue = 0.7 },
                new Rivalry { Id = 91, TeamOneId = 60, TeamTwoId = 45, RivarlyValue = 0.6 },
                new Rivalry { Id = 92, TeamOneId = 33, TeamTwoId = 37, RivarlyValue = 0.85 },
                new Rivalry { Id = 93, TeamOneId = 33, TeamTwoId = 31, RivarlyValue = 0.7 }
            );
        }

    }

    public class MatchConfiguration : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> builder)
        {
            builder.Property(m => m.MatchStatus)
                   .HasConversion<int>() // Store as integer in database
                   .IsRequired();

            // Optional: Create an index for better query performance
            builder.HasIndex(m => m.MatchStatus);
        }
    }
}
