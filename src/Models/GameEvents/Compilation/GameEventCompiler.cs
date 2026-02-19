using System;
using System.Linq;
using VikingJamGame.Models.GameEvents.Commands;
using VikingJamGame.Models.GameEvents.Definitions;
using VikingJamGame.Models.GameEvents.Runtime;

namespace VikingJamGame.Models.GameEvents.Compilation;

public static class GameEventCompiler
{
    public static GameEvent Compile(GameEventDefinition definition, ICommandRegistry commands)
    {
        if (string.IsNullOrWhiteSpace(definition.Id))
        {
            throw new InvalidOperationException("Event Id is required.");
        }

        var options = definition.OptionDefinitions
            .Select(optionDefinition => CompileOption(definition.Id, optionDefinition, commands))
            .OrderBy(option => option.Order)
            .ToList();

        var duplicateOrder = options
            .GroupBy(option => option.Order)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateOrder is not null)
        {
            throw new InvalidOperationException(
                $"Event '{definition.Id}' has duplicate option Order={duplicateOrder.Key}.");
        }

        return new GameEvent
        {
            Id = definition.Id,
            Name = definition.Name,
            Description = definition.Description,
            Options = options
        };
    }

    private static GameEventOption CompileOption(
        string eventId,
        GameEventOptionDefinition optionDefinition,
        ICommandRegistry commands)
    {
        var requirements = GameEventDefinitionParser.ParsePairs(
            eventId,
            optionDefinition.Order,
            "Condition",
            optionDefinition.Condition);

        var costs = GameEventDefinitionParser.ParsePairs(
            eventId,
            optionDefinition.Order,
            "Cost",
            optionDefinition.Cost);

        requirements = GameEventOptionRequirements.Merge(requirements, costs);
        var command = GameEventDefinitionParser.ParseCommand(
            eventId,
            optionDefinition.Order,
            optionDefinition.CustomCommand,
            commands);

        return new GameEventOption
        {
            DisplayText = optionDefinition.DisplayText,
            ResolutionText = optionDefinition.ResolutionText,
            Order = optionDefinition.Order,
            DisplayCosts = optionDefinition.DisplayCosts,
            Requirements = requirements,
            Costs = costs,
            Command = command,
            NextEventId = NormalizeNextEventId(optionDefinition.NextEventId)
        };
    }

    private static string? NormalizeNextEventId(string? nextEventId) =>
        string.IsNullOrWhiteSpace(nextEventId) ? null : nextEventId.Trim();
}
