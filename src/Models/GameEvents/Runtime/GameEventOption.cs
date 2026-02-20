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

    public bool IsAvailable(PlayerInfo playerInfo, GameResources gameResources) =>
        GameStateStats.MeetsAll(playerInfo, gameResources, Requirements) &&
        GameStateStats.CanPayAll(playerInfo, gameResources, Costs);

    public void Execute(PlayerInfo playerInfo, GameResources gameResources)
    {
        if (!GameStateStats.CanPayAll(playerInfo, gameResources, Costs))
        {
            throw new InvalidOperationException("Option executed without being affordable.");
        }

        GameStateStats.PayAll(playerInfo, gameResources, Costs);
        Command.Execute(playerInfo, gameResources);
    }
}
