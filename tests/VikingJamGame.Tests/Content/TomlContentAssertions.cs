using Tomlyn;
using Tomlyn.Syntax;

namespace VikingJamGame.Tests.Content;

internal static class TomlContentAssertions
{
    public static void AssertAllTomlFilesAreSyntacticallyValid(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        var allTomlFiles = Directory
            .GetFiles(directoryPath, "*.toml", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.NotEmpty(allTomlFiles);

        var syntaxErrors = new List<string>();
        foreach (var tomlFilePath in allTomlFiles)
        {
            var fileContents = File.ReadAllText(tomlFilePath);
            DocumentSyntax parseResult = Toml.Parse(fileContents);
            if (!parseResult.HasErrors)
            {
                continue;
            }

            var filePathFromRoot = Path.GetRelativePath(
                ContentTestProjectPaths.ProjectRoot,
                tomlFilePath);

            var formattedDiagnostics = string.Join(
                Environment.NewLine,
                parseResult.Diagnostics.Select(diagnostic => $"  {diagnostic}"));

            syntaxErrors.Add(
                $"{filePathFromRoot}{Environment.NewLine}{formattedDiagnostics}");
        }

        Assert.True(
            syntaxErrors.Count == 0,
            $"Found TOML syntax errors:{Environment.NewLine}{string.Join(Environment.NewLine + Environment.NewLine, syntaxErrors)}");
    }
}
