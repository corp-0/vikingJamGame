using System.Collections.Generic;

namespace VikingJamGame.Models.GameEvents.Definitions;

public sealed record GameEventDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }

    public List<GameEventOptionDefinition> OptionDefinitions { get; init; } = [];
}
