using System;
using System.IO;
using Godot;

namespace VikingJamGame.Repositories.Items;

public static class ItemDirectoryResolver
{
    public const string EDITOR_ITEMS_RESOURCE_DIRECTORY = "res://src/definitions/items";
    public const string BUILD_ITEMS_RELATIVE_DIRECTORY = "definitions/items";

    public static string ResolveForRuntime(
        string editorItemsResourceDirectory = EDITOR_ITEMS_RESOURCE_DIRECTORY,
        string buildItemsRelativeDirectory = BUILD_ITEMS_RELATIVE_DIRECTORY)
    {
        var isEditor = OS.HasFeature("editor");
        var editorAbsoluteDirectory = isEditor
            ? ProjectSettings.GlobalizePath(editorItemsResourceDirectory)
            : string.Empty;
        var executablePath = isEditor ? string.Empty : OS.GetExecutablePath();

        return Resolve(
            isEditor,
            editorAbsoluteDirectory,
            executablePath,
            buildItemsRelativeDirectory);
    }

    public static string Resolve(
        bool isEditor,
        string editorItemsAbsoluteDirectory,
        string executablePath,
        string buildItemsRelativeDirectory = BUILD_ITEMS_RELATIVE_DIRECTORY)
    {
        if (isEditor)
        {
            if (string.IsNullOrWhiteSpace(editorItemsAbsoluteDirectory))
            {
                throw new InvalidOperationException(
                    "Editor item directory is empty. Expected an absolute path for 'res://src/definitions/items'.");
            }

            return Path.GetFullPath(editorItemsAbsoluteDirectory);
        }

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new InvalidOperationException(
                "Executable path is empty. Cannot resolve build item directory.");
        }

        var executableDirectory = Path.GetDirectoryName(executablePath);
        if (string.IsNullOrWhiteSpace(executableDirectory))
        {
            throw new InvalidOperationException(
                $"Could not determine executable directory from path '{executablePath}'.");
        }

        return Path.GetFullPath(Path.Combine(executableDirectory, buildItemsRelativeDirectory));
    }
}
