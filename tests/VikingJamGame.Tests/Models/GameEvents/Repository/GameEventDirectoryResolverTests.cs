using VikingJamGame.Repositories.GameEvents;

namespace VikingJamGame.Tests.Models.GameEvents.Repository;

public sealed class GameEventDirectoryResolverTests
{
    [Fact]
    public void Resolve_EditorMode_UsesEditorAbsoluteDirectory()
    {
        var editorAbsoluteDirectory = Path.Combine(
            Path.GetTempPath(),
            "VikingJamGame.Tests",
            "events");
        var expected = Path.GetFullPath(editorAbsoluteDirectory);

        var resolved = GameEventDirectoryResolver.Resolve(
            isEditor: true,
            editorEventsAbsoluteDirectory: editorAbsoluteDirectory,
            executablePath: string.Empty);

        Assert.Equal(expected, resolved, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolve_BuildMode_UsesExecutableDirectoryPlusRelativeDefinitionsFolder()
    {
        var executablePath = Path.Combine("C:\\Games\\VikingJam", "VikingJamGame.exe");
        var expected = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(executablePath)!,
            "definitions",
            "events"));

        var resolved = GameEventDirectoryResolver.Resolve(
            isEditor: false,
            editorEventsAbsoluteDirectory: string.Empty,
            executablePath: executablePath);

        Assert.Equal(expected, resolved, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolve_EditorMode_ThrowsWhenEditorDirectoryIsMissing()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            GameEventDirectoryResolver.Resolve(
                isEditor: true,
                editorEventsAbsoluteDirectory: "",
                executablePath: ""));

        Assert.Contains("Editor event directory is empty", exception.Message);
    }

    [Fact]
    public void Resolve_BuildMode_ThrowsWhenExecutablePathIsMissing()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            GameEventDirectoryResolver.Resolve(
                isEditor: false,
                editorEventsAbsoluteDirectory: "",
                executablePath: ""));

        Assert.Contains("Executable path is empty", exception.Message);
    }
}
