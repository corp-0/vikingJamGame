using VikingJamGame.Models;
using VikingJamGame.Models.GameEvents.Commands;
using VikingJamGame.Models.GameEvents.Runtime;
using VikingJamGame.Models.GameEvents.Stats;
using VikingJamGame.Tests.TestDoubles;

namespace VikingJamGame.Tests.Models.GameEvents.Runtime;

public sealed class GameEventOptionTests
{
    [Fact]
    public void IsAvailable_ReturnsFalseWhenRequirementsAreNotMet()
    {
        var state = new GameState();
        state.AddFood(1);
        state.AddGold(2);

        GameEventOption option = CreateOption(
            requirements: [new StatAmount(StatId.Food, 2)],
            costs: [new StatAmount(StatId.Gold, 1)],
            command: NoopCommand.Instance);

        Assert.False(option.IsAvailable(state));
    }

    [Fact]
    public void Execute_PaysCostsAndRunsCommand()
    {
        var state = new GameState();
        state.AddFood(5);
        state.AddGold(4);
        var command = new RecordingCommand();
        GameEventOption option = CreateOption(
            requirements: [new StatAmount(StatId.Food, 2)],
            costs: [new StatAmount(StatId.Food, 2), new StatAmount(StatId.Gold, 1)],
            command: command);

        option.Execute(state);

        Assert.Equal(3, state.Food);
        Assert.Equal(3, state.Gold);
        Assert.Equal(1, command.ExecuteCalls);
    }

    [Fact]
    public void Execute_ThrowsWhenOptionIsNotAffordable()
    {
        var state = new GameState();
        state.AddFood(1);
        GameEventOption option = CreateOption(
            requirements: [new StatAmount(StatId.Food, 1)],
            costs: [new StatAmount(StatId.Food, 2)],
            command: NoopCommand.Instance);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => option.Execute(state));

        Assert.Contains("without being affordable", exception.Message);
    }

    private static GameEventOption CreateOption(
        IReadOnlyList<StatAmount> requirements,
        IReadOnlyList<StatAmount> costs,
        IEventCommand command) =>
        new()
        {
            DisplayText = "Option",
            ResolutionText = "Resolved",
            Order = 1,
            DisplayCosts = true,
            Requirements = requirements,
            Costs = costs,
            Command = command
        };
}
