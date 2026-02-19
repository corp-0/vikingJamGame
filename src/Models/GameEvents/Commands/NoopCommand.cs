namespace VikingJamGame.Models.GameEvents.Commands;

public sealed class NoopCommand : 
    IEventCommand
{
    public static readonly NoopCommand Instance = new();

    private NoopCommand() { }

    public void Execute(GameState state) { }
}
