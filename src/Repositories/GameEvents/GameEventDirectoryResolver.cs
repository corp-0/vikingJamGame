using System;
using System.IO;
using Godot;

namespace VikingJamGame.Repositories.GameEvents;

public static class GameEventDirectoryResolver
{
    public const string EDITOR_EVENTS_RESOURCE_DIRECTORY = "res://src/definitions/events";
    public const string BUILD_EVENTS_RELATIVE_DIRECTORY = "definitions/events";

    public static string ResolveForRuntime(
        string editorEventsResourceDirectory = EDITOR_EVENTS_RESOURCE_DIRECTORY,
        string buildEventsRelativeDirectory = BUILD_EVENTS_RELATIVE_DIRECTORY)
    {
        var isEditor = OS.HasFeature("editor");
        var editorAbsoluteDirectory = isEditor
            ? ProjectSettings.GlobalizePath(editorEventsResourceDirectory)
            : string.Empty;
        var executablePath = isEditor ? string.Empty : OS.GetExecutablePath();

        return Resolve(
            isEditor,
            editorAbsoluteDirectory,
            executablePath,
            buildEventsRelativeDirectory);
    }

    public static string Resolve(
        bool isEditor,
        string editorEventsAbsoluteDirectory,
        string executablePath,
        string buildEventsRelativeDirectory = BUILD_EVENTS_RELATIVE_DIRECTORY)
    {
        if (isEditor)
        {
            if (string.IsNullOrWhiteSpace(editorEventsAbsoluteDirectory))
            {
                throw new InvalidOperationException(
                    "Editor event directory is empty. Expected an absolute path for 'res://src/definitions/events'.");
            }

            return Path.GetFullPath(editorEventsAbsoluteDirectory);
        }

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new InvalidOperationException(
                "Executable path is empty. Cannot resolve build event directory.");
        }

        var executableDirectory = Path.GetDirectoryName(executablePath);
        if (string.IsNullOrWhiteSpace(executableDirectory))
        {
            throw new InvalidOperationException(
                $"Could not determine executable directory from path '{executablePath}'.");
        }

        return Path.GetFullPath(Path.Combine(executableDirectory, buildEventsRelativeDirectory));
    }
}
