using BetterConsoleTables;
using important_game.ui;
using important_game.ui.Infrastructure.ImportantMatch;
using Microsoft.Extensions.DependencyInjection;

IServiceCollection services = new ServiceCollection();

services.MatchImportanceDependency();

var serviceProvider = services.BuildServiceProvider();
var matchProcessor = serviceProvider.GetService<IExcitmentMatchProcessor>();

Console.WriteLine("Welcome to Match Processor");
Console.WriteLine("==========================");

Console.WriteLine("Analyzing the following leagues:");


var excitementMatches = await matchProcessor.GetUpcomingExcitementMatchesAsync(new MatchImportanceOptions());


Console.Clear();
var table = new Table("League", "Date", "Match", "Importance");

foreach (var match in excitementMatches.OrderByDescending(c => c.ExcitementScore).ThenBy(c => c.MatchDate))
{
    table.AddRow(match.League.Name
        , match.MatchDate.ToString("yyyy-MM-dd HH:mm")
        , $"{match.HomeTeam.Name} v {match.AwayTeam.Name}"
        , Math.Round(match.ExcitementScore, 3));
}

Console.Write(table.ToString());


Console.WriteLine("==========================");
Console.WriteLine("Match Processor Finished");

Console.ReadKey();
