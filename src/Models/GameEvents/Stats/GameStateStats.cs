using System;
using System.Collections.Generic;

namespace VikingJamGame.Models.GameEvents.Stats;

internal static class GameStateStats
{
    public static bool MeetsAll(GameState state, IReadOnlyList<StatAmount> requirements)
    {
        foreach (StatAmount requirement in requirements)
        {
            if (Get(state, requirement.Stat) < requirement.Amount)
            {
                return false;
            }
        }

        return true;
    }

    public static bool CanPayAll(GameState state, IReadOnlyList<StatAmount> costs)
    {
        foreach (StatAmount cost in costs)
        {
            if (Get(state, cost.Stat) < cost.Amount)
            {
                return false;
            }
        }

        return true;
    }

    public static void PayAll(GameState state, IReadOnlyList<StatAmount> costs)
    {
        foreach (StatAmount cost in costs)
        {
            Spend(state, cost.Stat, cost.Amount);
        }
    }

    public static int Get(GameState state, StatId id) => id switch
    {
        StatId.Population => state.Population,
        StatId.Food => state.Food,
        StatId.Gold => state.Gold,
        StatId.Strength => state.Strength,
        StatId.Honor => state.Honor,
        StatId.Feats => state.Feats,
        _ => throw new ArgumentOutOfRangeException(nameof(id))
    };

    private static void Spend(GameState state, StatId id, int amount)
    {
        if (amount < 0)
        {
            throw new InvalidOperationException("Cost amount must be >= 0.");
        }

        switch (id)
        {
            case StatId.Population:
                state.RemovePopulation(amount);
                break;
            case StatId.Food:
                state.SpendFood(amount);
                break;
            case StatId.Gold:
                state.SpendGold(amount);
                break;
            case StatId.Strength:
                state.RemoveStrength(amount);
                break;
            case StatId.Honor:
                state.RemoveHonor(amount);
                break;
            case StatId.Feats:
                state.RemoveFeats(amount);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(id));
        }
    }
}
