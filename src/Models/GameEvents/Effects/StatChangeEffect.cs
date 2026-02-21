using VikingJamGame.Models.GameEvents.Stats;

namespace VikingJamGame.Models.GameEvents.Effects;

/// <summary>
/// Adds or removes a stat amount. Positive = gain, negative = loss.
/// </summary>
public sealed record StatChangeEffect(StatId Stat, int Amount) : IGameEventEffect
{
    public bool IsPositive => Amount >= 0;

    public void Apply(GameEventContext context)
    {
        if (Amount >= 0)
        {
            GameStateStats.Add(context.PlayerInfo, context.GameResources, Stat, Amount);
        }
        else
        {
            GameStateStats.Spend(context.PlayerInfo, context.GameResources, Stat, -Amount);
        }
    }

    public string GetDisplayText(GameEventContext context)
    {
        string sign = Amount >= 0 ? "+" : "";
        return $"{sign}{Amount} {Stat}";
    }
}
