using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace important_game.infrastructure.Contexts.ScoreCalculator.Calculators;
/// <summary>
/// Interface for calculating title holder advantage value.
/// </summary>
public interface ITitleHolderCalculator
{
    /// <summary>
    /// Calculates title holder score if either team is the defending champion.
    /// Returns 1 if either team is title holder, 0 otherwise.
    /// </summary>
    double CalculateTitleHolderScore(int homeTeamId, int awayTeamId, int? titleHolderId);
}
/// <summary>
/// Calculates title holder advantage.
/// </summary>
[ExcludeFromCodeCoverage]
internal class TitleHolderCalculator : ITitleHolderCalculator
{
    public double CalculateTitleHolderScore(int homeTeamId, int awayTeamId, int? titleHolderId)
    {
        if (!titleHolderId.HasValue)
            return 0d;

        return homeTeamId == titleHolderId.Value || awayTeamId == titleHolderId.Value ? 1d : 0d;
    }
}

