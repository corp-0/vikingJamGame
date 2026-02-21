using System.Collections.Generic;

namespace VikingJamGame.Models.Items;
// blueprint object to read from the TOML
public class ItemDefinition
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    // just the name and file extension. The file will be found on res://resources/items
    // default to Godot icon if not found
    public string Art { get; set; } = "null";
    // cursed item cannot be unequiped
    public bool IsCursed { get; init; } = false;
    // -1 means it is not consumable
    public int ConsumableCharges { get; init; } = -1;
    
    // each entry is a "token:value" pair, e.g. ["food:+3", "honor:-2"]
    public List<string> EffectsOnUse { get; init; } = [];
    public List<string> EffectsOnEquip { get; init; } = [];
    public List<string> EffectsOnUnequip { get; init; } = [];
}