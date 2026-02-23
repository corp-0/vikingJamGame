namespace VikingJamGame.Tests.Content;

internal static class ContentTestProjectPaths
{
    private static readonly Lazy<string> ProjectRootPath = new(ResolveProjectRootPath);

    public static string ProjectRoot => ProjectRootPath.Value;

    public static string ResolvePathFromProjectRoot(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        var normalizedRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.GetFullPath(Path.Combine(ProjectRoot, normalizedRelativePath));
    }

    private static string ResolveProjectRootPath()
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        while (currentDirectory is not null)
        {
            var containsSolutionFile = File.Exists(Path.Combine(currentDirectory.FullName, "VikingJamGame.sln"));
            var containsGodotProject = File.Exists(Path.Combine(currentDirectory.FullName, "project.godot"));
            if (containsSolutionFile && containsGodotProject)
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not locate repository root from test base directory '{AppContext.BaseDirectory}'.");
    }
}
