namespace VikingJamGame.Models.GameEvents.Commands;

public interface IEventCommand
{
    void Execute(GameState state);
}
