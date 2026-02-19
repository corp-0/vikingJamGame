using System;
using System.Collections.Generic;
using System.Linq;
using VikingJamGame.Models.GameEvents.Stats;

namespace VikingJamGame.Models.GameEvents.Compilation;

internal static class GameEventOptionRequirements
{
    public static IReadOnlyList<StatAmount> Merge(
        IReadOnlyList<StatAmount> requirements,
        IReadOnlyList<StatAmount> costs)
    {
        if (costs.Count == 0)
        {
            return requirements;
        }

        if (requirements.Count == 0)
        {
            return costs;
        }

        var maxPerStat = new Dictionary<StatId, int>();
        foreach (var requirement in requirements)
        {
            maxPerStat[requirement.Stat] = Math.Max(maxPerStat.GetValueOrDefault(requirement.Stat), requirement.Amount);
        }

        foreach (var cost in costs)
        {
            maxPerStat[cost.Stat] = Math.Max(maxPerStat.GetValueOrDefault(cost.Stat), cost.Amount);
        }

        return maxPerStat.Select(entry => new StatAmount(entry.Key, entry.Value)).ToList();
    }
}
