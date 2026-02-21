using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;
using VikingJamGame.Models.GameEvents.Compilation;
using VikingJamGame.Models.GameEvents.Definitions;
using VikingJamGame.Models.GameEvents.Runtime;

namespace VikingJamGame.Repositories.GameEvents;

public static class TomlGameEventRepositoryLoader
{
    public const string DEFAULT_EVENTS_DIRECTORY = "src/definitions/events";

    public static IGameEventRepository LoadFromDefaultDirectory(
        GameEventTemplateContext? templateContext = null) =>
        LoadFromDirectory(DEFAULT_EVENTS_DIRECTORY, templateContext);

    public static IGameEventRepository LoadFromDirectory(
        string directoryPath,
        GameEventTemplateContext? templateContext = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        var fullDirectoryPath = Path.GetFullPath(directoryPath);
        if (!Directory.Exists(fullDirectoryPath))
        {
            throw new DirectoryNotFoundException(
                $"Event definitions directory was not found: '{fullDirectoryPath}'.");
        }

        var eventFiles = Directory.GetFiles(fullDirectoryPath, "*.toml", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

        var events = new List<GameEvent>();
        foreach (var eventFilePath in eventFiles)
        {
            var definition = ReadDefinition(eventFilePath);
            var compiledEvent = GameEventCompiler.Compile(definition, templateContext);
            events.Add(compiledEvent);
        }

        ValidateChainLinks(events);

        return new InMemoryGameEventRepository(events);
    }

    private static GameEventDefinition ReadDefinition(string filePath)
    {
        var fileContents = File.ReadAllText(filePath);
        DocumentSyntax parseResult = Toml.Parse(fileContents);
        if (parseResult.HasErrors)
        {
            var errors = string.Join(
                Environment.NewLine,
                parseResult.Diagnostics.Select(diagnostic => diagnostic.ToString()));

            throw new InvalidOperationException(
                $"Invalid TOML in '{filePath}'.{Environment.NewLine}{errors}");
        }

        if (Toml.ToModel(fileContents) is not { } root)
        {
            throw new InvalidOperationException(
                $"TOML file '{filePath}' root value must be a table.");
        }

        return MapDefinition(root, filePath);
    }

    private static GameEventDefinition MapDefinition(TomlTable root, string filePath)
    {
        return new GameEventDefinition
        {
            Id = GetRequiredString(root, nameof(GameEventDefinition.Id), filePath),
            Name = GetRequiredString(root, nameof(GameEventDefinition.Name), filePath),
            Description = GetRequiredString(root, nameof(GameEventDefinition.Description), filePath),
            OptionDefinitions = GetOptionDefinitions(root, filePath)
        };
    }

    private static List<GameEventOptionDefinition> GetOptionDefinitions(TomlTable root, string filePath)
    {
        if (!root.TryGetValue(nameof(GameEventDefinition.OptionDefinitions), out var rawOptions))
        {
            return [];
        }

        if (rawOptions is not TomlTableArray optionTables)
        {
            throw new InvalidOperationException(
                $"TOML file '{filePath}' key '{nameof(GameEventDefinition.OptionDefinitions)}' must be an array of tables.");
        }

        var options = new List<GameEventOptionDefinition>(optionTables.Count);
        foreach (TomlTable rawOption in optionTables)
        {
            if (rawOption is null)
            {
                throw new InvalidOperationException(
                    $"TOML file '{filePath}' has a non-table option definition.");
            }

            options.Add(new GameEventOptionDefinition
            {
                DisplayText = GetRequiredString(
                    rawOption,
                    nameof(GameEventOptionDefinition.DisplayText),
                    filePath),
                ResolutionText = GetRequiredString(
                    rawOption,
                    nameof(GameEventOptionDefinition.ResolutionText),
                    filePath),
                Order = GetRequiredInt(
                    rawOption,
                    nameof(GameEventOptionDefinition.Order),
                    filePath),
                Conditions = GetOptionalStringArray(
                    rawOption,
                    nameof(GameEventOptionDefinition.Conditions),
                    filePath),
                Costs = GetOptionalStringArray(
                    rawOption,
                    nameof(GameEventOptionDefinition.Costs),
                    filePath),
                DisplayCost = GetOptionalBool(
                    rawOption,
                    nameof(GameEventOptionDefinition.DisplayCost),
                    defaultValue: false,
                    filePath),
                Effects = GetOptionalStringArray(
                    rawOption,
                    nameof(GameEventOptionDefinition.Effects),
                    filePath),
                NextEventId = GetOptionalString(
                    rawOption,
                    nameof(GameEventOptionDefinition.NextEventId),
                    filePath)
            });
        }

        return options;
    }

    private static string GetRequiredString(TomlTable table, string key, string filePath)
    {
        if (!table.TryGetValue(key, out var rawValue))
        {
            throw new InvalidOperationException(
                $"TOML file '{filePath}' is missing required key '{key}'.");
        }

        if (rawValue is not string value)
        {
            throw new InvalidOperationException(
                $"TOML file '{filePath}' key '{key}' must be a string.");
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"TOML file '{filePath}' key '{key}' cannot be empty.");
        }

        return value;
    }

    private static string? GetOptionalString(TomlTable table, string key, string filePath)
    {
        if (!table.TryGetValue(key, out var rawValue))
        {
            return null;
        }

        if (rawValue is not string value)
        {
            throw new InvalidOperationException(
                $"TOML file '{filePath}' key '{key}' must be a string.");
        }

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static bool GetOptionalBool(
        TomlTable table,
        string key,
        bool defaultValue,
        string filePath)
    {
        if (!table.TryGetValue(key, out var rawValue))
        {
            return defaultValue;
        }

        if (rawValue is bool value)
        {
            return value;
        }

        throw new InvalidOperationException(
            $"TOML file '{filePath}' key '{key}' must be a boolean.");
    }

    private static List<string> GetOptionalStringArray(TomlTable table, string key, string filePath)
    {
        if (!table.TryGetValue(key, out var rawValue))
        {
            return [];
        }

        if (rawValue is not TomlArray values)
        {
            throw new InvalidOperationException(
                $"TOML file '{filePath}' key '{key}' must be an array.");
        }

        var result = new List<string>(values.Count);
        foreach (var rawEntry in values)
        {
            if (rawEntry is not string value)
            {
                throw new InvalidOperationException(
                    $"TOML file '{filePath}' key '{key}' must contain only strings.");
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                result.Add(value);
            }
        }

        return result;
    }

    private static int GetRequiredInt(TomlTable table, string key, string filePath)
    {
        if (!table.TryGetValue(key, out var rawValue))
        {
            throw new InvalidOperationException(
                $"TOML file '{filePath}' is missing required key '{key}'.");
        }

        return rawValue switch
        {
            int value => value,
            long value => checked((int)value),
            _ => throw new InvalidOperationException(
                $"TOML file '{filePath}' key '{key}' must be an integer.")
        };
    }

    private static void ValidateChainLinks(IReadOnlyCollection<GameEvent> events)
    {
        var knownIds = events
            .Select(gameEvent => gameEvent.Id)
            .ToHashSet(StringComparer.Ordinal);

        foreach (GameEvent gameEvent in events)
        {
            foreach (GameEventOption option in gameEvent.Options)
            {
                if (string.IsNullOrWhiteSpace(option.NextEventId))
                {
                    continue;
                }

                if (!knownIds.Contains(option.NextEventId))
                {
                    throw new InvalidOperationException(
                        $"Event '{gameEvent.Id}' option {option.Order} points to missing NextEventId '{option.NextEventId}'.");
                }
            }
        }
    }
}
