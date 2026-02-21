using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;
using VikingJamGame.Models.Items;

namespace VikingJamGame.Repositories.Items;

public static class TomlItemRepositoryLoader
{
    public const string DEFAULT_ITEMS_DIRECTORY = "src/definitions/items";

    public static IItemRepository LoadFromDefaultDirectory() =>
        LoadFromDirectory(DEFAULT_ITEMS_DIRECTORY);

    public static IItemRepository LoadFromDirectory(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        var fullDirectoryPath = Path.GetFullPath(directoryPath);
        if (!Directory.Exists(fullDirectoryPath))
        {
            throw new DirectoryNotFoundException(
                $"Item definitions directory was not found: '{fullDirectoryPath}'.");
        }

        var itemFiles = Directory.GetFiles(fullDirectoryPath, "*.toml", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

        var items = new List<Item>();
        foreach (var itemFilePath in itemFiles)
        {
            var definition = ReadDefinition(itemFilePath);
            var compiledItem = ItemCompiler.Compile(definition);
            items.Add(compiledItem);
        }

        return new InMemoryItemRepository(items);
    }

    private static ItemDefinition ReadDefinition(string filePath)
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

        if (Toml.ToModel(fileContents) is not TomlTable root)
        {
            throw new InvalidOperationException(
                $"TOML file '{filePath}' root value must be a table.");
        }

        return new ItemDefinition
        {
            Id = GetRequiredString(root, nameof(ItemDefinition.Id), filePath),
            Name = GetRequiredString(root, nameof(ItemDefinition.Name), filePath),
            Description = GetRequiredString(root, nameof(ItemDefinition.Description), filePath),
            Art = GetOptionalString(root, nameof(ItemDefinition.Art), filePath) ?? "null",
            IsCursed = GetOptionalBool(root, nameof(ItemDefinition.IsCursed), defaultValue: false, filePath),
            ConsumableCharges = GetOptionalInt(root, nameof(ItemDefinition.ConsumableCharges), defaultValue: -1, filePath),
            EffectsOnUse = GetOptionalStringArray(root, nameof(ItemDefinition.EffectsOnUse), filePath),
            EffectsOnEquip = GetOptionalStringArray(root, nameof(ItemDefinition.EffectsOnEquip), filePath),
            EffectsOnUnequip = GetOptionalStringArray(root, nameof(ItemDefinition.EffectsOnUnequip), filePath),
        };
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

    private static bool GetOptionalBool(TomlTable table, string key, bool defaultValue, string filePath)
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

    private static int GetOptionalInt(TomlTable table, string key, int defaultValue, string filePath)
    {
        if (!table.TryGetValue(key, out var rawValue))
        {
            return defaultValue;
        }

        return rawValue switch
        {
            int value => value,
            long value => checked((int)value),
            _ => throw new InvalidOperationException(
                $"TOML file '{filePath}' key '{key}' must be an integer.")
        };
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
}
