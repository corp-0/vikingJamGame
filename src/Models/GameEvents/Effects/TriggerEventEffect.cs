namespace VikingJamGame.Models.GameEvents.Effects;

/// <summary>
/// Sets the triggered event ID on the context, causing that event to fire next.
/// Works like NextEventId on options.
/// </summary>
public sealed record TriggerEventEffect(string EventId) : IGameEventEffect
{
    public bool IsPositive => true;

    public void Apply(GameEventContext context) =>
        context.TriggeredEventId = EventId;

    /// <summary>Event triggers have no visible resolution text.</summary>
    public string? GetDisplayText(GameEventContext context) => null;
}
