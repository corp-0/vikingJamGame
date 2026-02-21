using System;
using System.Linq;
using VikingJamGame.Models.GameEvents.Definitions;
using VikingJamGame.Models.GameEvents.Runtime;

namespace VikingJamGame.Models.GameEvents.Compilation;

public static class GameEventCompiler
{
    public static GameEvent Compile(
        GameEventDefinition definition,
        GameEventTemplateContext? templateContext = null)
    {
        if (string.IsNullOrWhiteSpace(definition.Id))
        {
            throw new InvalidOperationException("Event Id is required.");
        }

        var options = definition.OptionDefinitions
            .Select(optionDefinition => CompileOption(
                definition.Id,
                optionDefinition,
                templateContext))
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
            Name = GameEventDefinitionParser.ParseTemplatedText(definition.Name, templateContext),
            Description = GameEventDefinitionParser.ParseTemplatedText(definition.Description, templateContext),
            Options = options
        };
    }

    private static GameEventOption CompileOption(
        string eventId,
        GameEventOptionDefinition optionDefinition,
        GameEventTemplateContext? templateContext)
    {
        var visibilityConditions = GameEventDefinitionParser.ParseConditionPairs(
            eventId,
            optionDefinition.Order,
            "Conditions",
            optionDefinition.Conditions);

        var costs = GameEventDefinitionParser.ParsePairs(
            eventId,
            optionDefinition.Order,
            "Costs",
            optionDefinition.Costs);

        var effects = GameEventDefinitionParser.ParseEffectPairs(
                eventId,
                optionDefinition.Order,
                "Effects",
                optionDefinition.Effects);

        return new GameEventOption
        {
            DisplayText = GameEventDefinitionParser.ParseTemplatedText(
                optionDefinition.DisplayText,
                templateContext),
            ResolutionText = GameEventDefinitionParser.ParseTemplatedText(
                optionDefinition.ResolutionText,
                templateContext),
            Order = optionDefinition.Order,
            DisplayCost = optionDefinition.DisplayCost,
            VisibilityConditions = visibilityConditions,
            Costs = costs,
            Effects = effects,
            NextEventId = NormalizeNextEventId(optionDefinition.NextEventId)
        };
    }

    private static string? NormalizeNextEventId(string? nextEventId) =>
        string.IsNullOrWhiteSpace(nextEventId) ? null : nextEventId.Trim();
}
