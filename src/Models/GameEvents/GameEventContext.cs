using VikingJamGame.Repositories.Items;

namespace VikingJamGame.Models.GameEvents;

public sealed class GameEventContext
{
    public required PlayerInfo PlayerInfo { get; init; }
    public required GameResources GameResources { get; init; }
    public required IItemRepository ItemRepository { get; init; }
    public string? CurrentNodeKind { get; init; }

    /// <summary>Set by TriggerEventEffect. Works like NextEventId on options.</summary>
    public string? TriggeredEventId { get; set; }
}
