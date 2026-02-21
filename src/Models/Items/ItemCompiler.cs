using System;
using VikingJamGame.Models.GameEvents.Compilation;

namespace VikingJamGame.Models.Items;

public static class ItemCompiler
{
    public static Item Compile(ItemDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.Id))
        {
            throw new InvalidOperationException("Item Id is required.");
        }

        return new Item
        {
            Id = definition.Id,
            Name = definition.Name,
            Description = definition.Description,
            Art = definition.Art,
            IsCursed = definition.IsCursed,
            RemainingCharges = definition.ConsumableCharges,
            EffectsOnUse = GameEventDefinitionParser.ParseEffectPairs(
                definition.Id, 0, "EffectsOnUse", definition.EffectsOnUse),
            EffectsOnEquip = GameEventDefinitionParser.ParseEffectPairs(
                definition.Id, 0, "EffectsOnEquip", definition.EffectsOnEquip),
            EffectsOnUnequip = GameEventDefinitionParser.ParseEffectPairs(
                definition.Id, 0, "EffectsOnUnequip", definition.EffectsOnUnequip),
        };
    }
}
