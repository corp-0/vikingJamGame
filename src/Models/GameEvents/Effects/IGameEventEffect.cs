namespace VikingJamGame.Models.GameEvents.Effects;

public interface IGameEventEffect
{
    void Apply(GameEventContext context);

    /// <summary>Text shown in the resolution screen. Null means nothing to display.</summary>
    string? GetDisplayText(GameEventContext context);

    /// <summary>Whether this effect is positive (for label coloring).</summary>
    bool IsPositive { get; }
}
