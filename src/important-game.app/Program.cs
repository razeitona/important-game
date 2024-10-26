using BetterConsoleTables;
using important_game.infrastructure;
using important_game.infrastructure.ImportantMatch;
using important_game.infrastructure.ImportantMatch.Live;
using Microsoft.Extensions.DependencyInjection;

IServiceCollection services = new ServiceCollection();

services.MatchImportanceInfrastructure();

var serviceProvider = services.BuildServiceProvider();
var matchProcessor = serviceProvider.GetService<IExcitmentMatchProcessor>();
var liveProcessor = serviceProvider.GetService<IExcitmentMatchLiveProcessor>();

Console.WriteLine("Welcome to Match Processor");
Console.WriteLine("==========================");

Console.WriteLine("Analyzing the following leagues:");

var brest = await liveProcessor.ProcessLiveMatchData(12764435);
var psg = await liveProcessor.ProcessLiveMatchData(12764392);
var sporting = await liveProcessor.ProcessLiveMatchData(12764511);
var girona = await liveProcessor.ProcessLiveMatchData(12764255);
var arsenal = await liveProcessor.ProcessLiveMatchData(12763955);
//

var excitementMatches = await matchProcessor.GetUpcomingExcitementMatchesAsync(new ExctimentMatchOptions());


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
