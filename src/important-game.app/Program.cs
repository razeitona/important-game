using important_game.ui.Core;
using important_game.ui.Core.Models;
using important_game.ui.Domain.ImportantMatch;
using important_game.ui.Domain.SofaScoreAPI;
using important_game.ui.Infrastructure;
using important_game.ui.Infrastructure.ImportantMatch;
using Microsoft.Extensions.DependencyInjection;

IServiceCollection services = new ServiceCollection();
services.AddHttpClient<ISofaScoreIntegration, SofaScoreIntegration>();
services.AddScoped<IMatchProcessorService, MatchProcessorService>();


var serviceProvider = services.BuildServiceProvider();
var matchProcessor = serviceProvider.GetService<IMatchProcessorService>();

Console.WriteLine("Welcome to Match Processor");
Console.WriteLine("==========================");

Console.WriteLine("Analyzing the following leagues:");

List<Task> importantFixturesMatch = new List<Task>();


foreach (var configLeague in MatchImportanceOptions.Leagues)
{
    Console.WriteLine($"{configLeague.Name}");

    var leagues = await matchProcessor.GetLeaguesAsync(configLeague.LeagueId);


    foreach (var league in leagues)
    {
        Console.WriteLine($"Start to process {league.Name} for season {league.CurrentSeason.Name}");
        var upcomingFixtures = await matchProcessor.GetUpcomingFixturesAsync(league.Id, league.CurrentSeason.Id);
        if (upcomingFixtures == null)
        {
            Console.WriteLine($"No upcoming features to process for {league.Name}");
            continue;
        }

        importantFixturesMatch.Add(ProcessUpcomingFixturesImportantMatches(configLeague, upcomingFixtures));
    }
}



await Task.WhenAll(importantFixturesMatch);


Console.WriteLine("==========================");
Console.WriteLine("Match Processor Finished");


async Task ProcessUpcomingFixturesImportantMatches(SofaScoreLeagueOption league, LeagueUpcomingFixtures leagueFixtures)
{
    Console.WriteLine($"Start process upcoming features for league {league.Name}");

    var matchCalculatorOptions = new ImportantMatchCalculatorOption { CompetitionRanking = league.Importance };

    foreach (var fixture in leagueFixtures)
    {
        var matchImportanceResult = await matchProcessor.CalculateMatchImportanceAsync(matchCalculatorOptions, fixture);

        Console.WriteLine($"{fixture.HomeTeam.Name} v {fixture.AwayTeam.Name}:  {matchImportanceResult.Importance}");

    }
}