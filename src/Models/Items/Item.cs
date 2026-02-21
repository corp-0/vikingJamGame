using System.Collections.Generic;
using VikingJamGame.Models.GameEvents.Effects;

namespace VikingJamGame.Models.Items;

// logical model to be used in-game
public class Item
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Art { get; init; }
    public required bool IsCursed { get; init; }
    /// <summary>-1 means not consumable. Decremented on each use.</summary>
    public int RemainingCharges { get; set; }
    public bool IsConsumable => RemainingCharges >= 0;
    public required IReadOnlyList<IGameEventEffect> EffectsOnUse { get; init; }
    public required IReadOnlyList<IGameEventEffect> EffectsOnEquip { get; init; }
    public required IReadOnlyList<IGameEventEffect> EffectsOnUnequip { get; init; }
}