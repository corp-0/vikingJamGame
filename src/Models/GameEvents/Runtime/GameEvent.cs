using System.Collections.Generic;

namespace VikingJamGame.Models.GameEvents.Runtime;

public sealed class GameEvent
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required IReadOnlyList<GameEventOption> Options { get; init; }
}
