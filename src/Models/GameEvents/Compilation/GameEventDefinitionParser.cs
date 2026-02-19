using System;
using System.Collections.Generic;
using System.Globalization;
using VikingJamGame.Models.GameEvents.Commands;
using VikingJamGame.Models.GameEvents.Stats;

namespace VikingJamGame.Models.GameEvents.Compilation;

internal static class GameEventDefinitionParser
{
    public static IReadOnlyList<StatAmount> ParsePairs(
        string eventId,
        int order,
        string fieldName,
        string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<StatAmount>();
        }

        var segments = text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var parsed = new List<StatAmount>(segments.Length);

        foreach (var segment in segments)
        {
            var pair = segment.Split(':', 2, StringSplitOptions.TrimEntries);
            if (pair.Length != 2)
            {
                throw new InvalidOperationException(
                    $"Event '{eventId}' option {order}: bad {fieldName} segment '{segment}'. Expected key:value.");
            }

            if (!TryParseStatId(pair[0], out var stat))
            {
                throw new InvalidOperationException(
                    $"Event '{eventId}' option {order}: unknown stat '{pair[0]}' in {fieldName}.");
            }

            if (!int.TryParse(pair[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount))
            {
                throw new InvalidOperationException(
                    $"Event '{eventId}' option {order}: bad amount '{pair[1]}' in {fieldName}.");
            }

            if (amount < 0)
            {
                throw new InvalidOperationException(
                    $"Event '{eventId}' option {order}: negative amount '{amount}' in {fieldName}.");
            }

            parsed.Add(new StatAmount(stat, amount));
        }

        return parsed;
    }

    public static IEventCommand ParseCommand(
        string eventId,
        int order,
        string? customCommand,
        ICommandRegistry commands)
    {
        if (string.IsNullOrWhiteSpace(customCommand))
        {
            return NoopCommand.Instance;
        }

        var parts = customCommand.Split(':', 2, StringSplitOptions.TrimEntries);
        var name = parts[0];
        var arg = parts.Length == 2 ? parts[1] : null;

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException($"Event '{eventId}' option {order}: empty CustomCommand.");
        }

        return commands.Create(name, arg);
    }

    private static bool TryParseStatId(string key, out StatId id)
    {
        switch (key.Trim().ToLowerInvariant())
        {
            case "population":
                id = StatId.Population;
                return true;
            case "food":
                id = StatId.Food;
                return true;
            case "gold":
                id = StatId.Gold;
                return true;
            case "strength":
                id = StatId.Strength;
                return true;
            case "honor":
                id = StatId.Honor;
                return true;
            case "feats":
                id = StatId.Feats;
                return true;
            default:
                id = default;
                return false;
        }
    }
}
