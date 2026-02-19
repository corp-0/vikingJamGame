namespace VikingJamGame.Models.GameEvents.Commands;

public interface ICommandRegistry
{
    IEventCommand Create(string name, string? arg);
}
