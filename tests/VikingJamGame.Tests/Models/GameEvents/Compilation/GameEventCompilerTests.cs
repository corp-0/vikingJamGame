using VikingJamGame.Models;
using VikingJamGame.Models.GameEvents.Commands;
using VikingJamGame.Models.GameEvents.Compilation;
using VikingJamGame.Models.GameEvents.Definitions;
using VikingJamGame.Models.GameEvents.Runtime;
using VikingJamGame.Models.GameEvents.Stats;
using VikingJamGame.Tests.TestDoubles;

namespace VikingJamGame.Tests.Models.GameEvents.Compilation;

public sealed class GameEventCompilerTests
{
    [Fact]
    public void Compile_SortsOptionsByOrder_AndMergesCostIntoRequirements()
    {
        var definition = new GameEventDefinition
        {
            Id = "event.alpha",
            Name = "Alpha",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "Second",
                    ResolutionText = "Second",
                    Order = 2
                },
                new GameEventOptionDefinition
                {
                    DisplayText = "First",
                    ResolutionText = "First",
                    Order = 1,
                    Condition = "food:1",
                    Cost = "food:3;gold:2"
                }
            ]
        };

        GameEvent compiled = GameEventCompiler.Compile(definition, new RecordingCommandRegistry());

        Assert.Equal([1, 2], compiled.Options.Select(option => option.Order).ToArray());
        GameEventOption first = compiled.Options.First();
        Assert.Contains(first.Requirements, amount => amount is { Stat: StatId.Food, Amount: 3 });
        Assert.Contains(first.Requirements, amount => amount is { Stat: StatId.Gold, Amount: 2 });
        Assert.Equal(2, first.Requirements.Count);
    }

    [Fact]
    public void Compile_ThrowsWhenOptionOrderIsDuplicated()
    {
        var definition = new GameEventDefinition
        {
            Id = "event.dup",
            Name = "Dup",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "A",
                    ResolutionText = "A",
                    Order = 1
                },
                new GameEventOptionDefinition
                {
                    DisplayText = "B",
                    ResolutionText = "B",
                    Order = 1
                }
            ]
        };

        InvalidOperationException exception = Assert
            .Throws<InvalidOperationException>(() => GameEventCompiler.Compile(definition, new RecordingCommandRegistry()));

        Assert.Contains("duplicate option Order=1", exception.Message);
    }

    [Fact]
    public void Compile_UsesCommandRegistryForCustomCommand()
    {
        var markerCommand = new RecordingCommand();
        var commandRegistry = new RecordingCommandRegistry(markerCommand);
        var definition = new GameEventDefinition
        {
            Id = "event.command",
            Name = "Command",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "Do",
                    ResolutionText = "Done",
                    Order = 1,
                    CustomCommand = "ApplyDebuff:Fear"
                }
            ]
        };

        GameEvent compiled = GameEventCompiler.Compile(definition, commandRegistry);

        Assert.Single(commandRegistry.CreatedCommands);
        Assert.Equal(("ApplyDebuff", "Fear"), commandRegistry.CreatedCommands[0]);
        Assert.Same(markerCommand, compiled.Options.Single().Command);
    }

    [Fact]
    public void Compile_UsesNoopWhenCustomCommandIsMissing()
    {
        var commandRegistry = new RecordingCommandRegistry();
        var definition = new GameEventDefinition
        {
            Id = "event.noop",
            Name = "Noop",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "Wait",
                    ResolutionText = "Waited",
                    Order = 1
                }
            ]
        };

        GameEvent compiled = GameEventCompiler.Compile(definition, commandRegistry);

        Assert.Empty(commandRegistry.CreatedCommands);
        Assert.Same(NoopCommand.Instance, compiled.Options.Single().Command);
    }

    [Fact]
    public void Compile_ThrowsWhenConditionUsesUnknownStat()
    {
        var definition = new GameEventDefinition
        {
            Id = "event.badstat",
            Name = "Bad Stat",
            Description = "desc",
            OptionDefinitions =
            [
                new GameEventOptionDefinition
                {
                    DisplayText = "X",
                    ResolutionText = "Y",
                    Order = 1,
                    Condition = "mystery:2"
                }
            ]
        };

        InvalidOperationException exception = Assert.
            Throws<InvalidOperationException>(() => GameEventCompiler.Compile(definition, new RecordingCommandRegistry()));

        Assert.Contains("unknown stat 'mystery' in Condition", exception.Message);
    }
}
