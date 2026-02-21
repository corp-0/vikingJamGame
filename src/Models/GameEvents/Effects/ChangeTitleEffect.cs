namespace VikingJamGame.Models.GameEvents.Effects;

/// <summary>
/// Changes the player's title.
/// </summary>
public sealed record ChangeTitleEffect(string NewTitle) : IGameEventEffect
{
    public bool IsPositive => true;

    public void Apply(GameEventContext context) =>
        context.PlayerInfo.SetTitle(NewTitle);

    public string GetDisplayText(GameEventContext context) =>
        $"You are known as {NewTitle}";
}
