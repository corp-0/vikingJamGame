using System;
using System.Collections.Generic;
using VikingJamGame.Models.GameEvents.Commands;
using VikingJamGame.Models.GameEvents.Stats;

namespace VikingJamGame.Models.GameEvents.Runtime;

public sealed class GameEventOption
{
    public required string DisplayText { get; init; }
    public required string ResolutionText { get; init; }
    public required int Order { get; init; }
    public required bool DisplayCosts { get; init; }

    public required IReadOnlyList<StatAmount> Requirements { get; init; }
    public required IReadOnlyList<StatAmount> Costs { get; init; }

    public required IEventCommand Command { get; init; }
    public string? NextEventId { get; init; }

    public bool IsAvailable(GameState state) =>
        GameStateStats.MeetsAll(state, Requirements) &&
        GameStateStats.CanPayAll(state, Costs);

    public void Execute(GameState state)
    {
        if (!GameStateStats.CanPayAll(state, Costs))
        {
            throw new InvalidOperationException("Option executed without being affordable.");
        }

        GameStateStats.PayAll(state, Costs);
        Command.Execute(state);
    }
}
