using System;
using System.Collections.Generic;
using System.Globalization;
using VikingJamGame.Models.GameEvents.Conditions;
using VikingJamGame.Models.GameEvents.Effects;
using VikingJamGame.Models.GameEvents.Stats;
using VikingJamGame.TemplateUtils;

namespace VikingJamGame.Models.GameEvents.Compilation;

internal static class GameEventDefinitionParser
{
    public static string ParseTemplatedText(
        string text,
        GameEventTemplateContext? templateContext = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        GameEventTemplateContext context = templateContext ?? GameEventTemplateContext.Default;
        string name = string.IsNullOrWhiteSpace(context.Name) ? "{Name}" : context.Name;
        string title = string.IsNullOrWhiteSpace(context.Title) ? "{Title}" : context.Title;

        return Template.Render(text, context.BirthChoice, name, title);
    }

    /// <summary>
    /// Parses "stat:amount" pairs where amounts must be non-negative. Used for costs.
    /// </summary>
    public static IReadOnlyList<StatAmount> ParsePairs(
        string sourceId,
        int order,
        string fieldName,
        IReadOnlyList<string> entries)
    {
        if (entries.Count == 0)
        {
            return [];
        }

        var parsed = new List<StatAmount>(entries.Count);

        foreach (var entry in entries)
        {
            var (stat, amount) = ParseStatPair(sourceId, order, fieldName, entry, allowNegative: false);
            parsed.Add(new StatAmount(stat, amount));
        }

        return parsed;
    }

    /// <summary>
    /// Parses "key:value" condition pairs. Dispatches to the appropriate condition type by key prefix.
    /// Stat names (food, gold, ...) → StatThresholdCondition.
    /// "item", "title", "node_kind" → respective condition stubs.
    /// </summary>
    public static IReadOnlyList<IGameEventCondition> ParseConditionPairs(
        string sourceId,
        int order,
        string fieldName,
        IReadOnlyList<string> entries)
    {
        if (entries.Count == 0)
        {
            return [];
        }

        var parsed = new List<IGameEventCondition>(entries.Count);

        foreach (var entry in entries)
        {
            var pair = entry.Split(':', 2, StringSplitOptions.TrimEntries);
            if (pair.Length != 2)
            {
                throw new InvalidOperationException(
                    $"'{sourceId}' option {order}: bad {fieldName} entry '{entry}'. Expected token:value.");
            }

            parsed.Add(ParseCondition(sourceId, order, fieldName, pair[0], pair[1]));
        }

        return parsed;
    }

    private static IGameEventCondition ParseCondition(
        string eventId, int order, string fieldName, string key, string value)
    {
        switch (key.Trim().ToLowerInvariant())
        {
            case "item":
                return new HasItemCondition(value.Trim());
            case "title":
                return new HasTitleCondition(value.Trim());
            case "node_kind":
                return new NodeKindCondition(value.Trim());
            default:
                if (TryParseStatId(key, out var stat))
                {
                    if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount)
                        || amount < 0)
                    {
                        throw new InvalidOperationException(
                            $"Event '{eventId}' option {order}: bad amount '{value}' in {fieldName}.");
                    }
                    return new StatThresholdCondition(stat, amount);
                }
                throw new InvalidOperationException(
                    $"Event '{eventId}' option {order}: unknown condition key '{key}' in {fieldName}.");
        }
    }

    /// <summary>
    /// Parses signed "stat:+amount" or "stat:-amount" pairs as effects. Positive = gain, negative = loss.
    /// </summary>
    public static IReadOnlyList<IGameEventEffect> ParseEffectPairs(
        string sourceId,
        int order,
        string fieldName,
        IReadOnlyList<string> entries)
    {
        if (entries.Count == 0)
        {
            return [];
        }

        var parsed = new List<IGameEventEffect>(entries.Count);

        foreach (var entry in entries)
        {
            parsed.Add(ParseEffect(sourceId, order, fieldName, entry));
        }

        return parsed;
    }

    private static IGameEventEffect ParseEffect(
        string sourceId,
        int order,
        string fieldName,
        string segment)
    {
        var pair = segment.Split(':', 2, StringSplitOptions.TrimEntries);
        if (pair.Length != 2)
        {
            throw new InvalidOperationException(
                $"'{sourceId}' option {order}: bad {fieldName} segment '{segment}'. Expected token:value.");
        }

        var token = pair[0].Trim().ToLowerInvariant();
        var value = pair[1].Trim();

        switch (token)
        {
            case "event":
                RequireNonEmpty(sourceId, order, fieldName, token, value);
                return new TriggerEventEffect(value);

            case "item":
                RequireNonEmpty(sourceId, order, fieldName, token, value);
                return new GrantItemEffect(value);

            case "title":
                RequireNonEmpty(sourceId, order, fieldName, token, value);
                return new ChangeTitleEffect(value);

            default:
                if (TryParseStatId(token, out StatId stat))
                {
                    var amount = ParseSignedAmount(sourceId, order, fieldName, pair[1]);
                    return new StatChangeEffect(stat, amount);
                }

                throw new InvalidOperationException(
                    $"'{sourceId}' option {order}: unknown effect token '{token}' in {fieldName}.");
        }
    }

    private static void RequireNonEmpty(
        string sourceId, int order, string fieldName, string token, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"'{sourceId}' option {order}: empty value for '{token}' in {fieldName}.");
        }
    }

    private static int ParseSignedAmount(
        string sourceId, int order, string fieldName, string raw)
    {
        var trimmed = raw.Trim().TrimStart('+');
        if (!int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount))
        {
            throw new InvalidOperationException(
                $"'{sourceId}' option {order}: bad amount '{raw}' in {fieldName}.");
        }

        return amount;
    }



    private static (StatId stat, int amount) ParseStatPair(
        string eventId,
        int order,
        string fieldName,
        string segment,
        bool allowNegative)
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

        var valueStr = pair[1].TrimStart('+');
        if (!int.TryParse(valueStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount))
        {
            throw new InvalidOperationException(
                $"Event '{eventId}' option {order}: bad amount '{pair[1]}' in {fieldName}.");
        }

        if (!allowNegative && amount < 0)
        {
            throw new InvalidOperationException(
                $"Event '{eventId}' option {order}: negative amount '{amount}' in {fieldName}.");
        }

        return (stat, amount);
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
            case "max_strength":
                id = StatId.MaxStrength;
                return true;
            case "max_honor":
                id = StatId.MaxHonor;
                return true;
            case "max_feats":
                id = StatId.MaxFeats;
                return true;
            default:
                id = default;
                return false;
        }
    }
}
