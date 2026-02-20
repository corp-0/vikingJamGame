using VikingJamGame.Models;
using VikingJamGame.Models.GameEvents.Commands;

namespace VikingJamGame.Tests.TestDoubles;

public sealed class RecordingCommand : IEventCommand
{
    public int ExecuteCalls { get; private set; }

    public void Execute(PlayerInfo playerInfo, GameResources gameResources)
    {
        ExecuteCalls++;
    }
}

public sealed class RecordingCommandRegistry(IEventCommand? commandToReturn = null) : ICommandRegistry
{
    private readonly IEventCommand _commandToReturn = commandToReturn ?? NoopCommand.Instance;

    public List<(string Name, string? Arg)> CreatedCommands { get; } = [];

    public IEventCommand Create(string name, string? arg)
    {
        CreatedCommands.Add((name, arg));
        return _commandToReturn;
    }
}
