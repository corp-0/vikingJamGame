using System.Collections.Generic;

namespace VikingJamGame.Models.GameEvents.Definitions;

public sealed record GameEventOptionDefinition
{
    public required string DisplayText { get; init; }
    public required string ResolutionText { get; init; }
    public required int Order { get; init; }

    // each entry is a "token:value" pair, e.g. ["food:3", "gold:10"]
    public List<string> Conditions { get; init; } = [];

    // each entry is a "stat:amount" pair, e.g. ["food:3", "honor:1"]
    public List<string> Costs { get; init; } = [];
    public bool DisplayCost { get; init; } = false;

    // each entry is a "token:value" pair, e.g. ["food:+3", "honor:-1", "item:mead_flask"]
    public List<string> Effects { get; init; } = [];

    // ID of the next event in the chain when this option is chosen.
    public string? NextEventId { get; init; }
}
