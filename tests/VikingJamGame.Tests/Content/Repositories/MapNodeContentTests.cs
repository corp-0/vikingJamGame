using VikingJamGame.Repositories.Navigation;

namespace VikingJamGame.Tests.Content.Repositories;

public sealed class MapNodeContentTests
{
    [Fact]
    public void MapNodeDefinitions_AreSyntacticallyValid_AndCanBeLoaded()
    {
        var definitionsDirectory = ContentTestProjectPaths.ResolvePathFromProjectRoot(
            TomlMapNodeRepositoryLoader.DEFAULT_MAP_NODES_DIRECTORY);

        TomlContentAssertions.AssertAllTomlFilesAreSyntacticallyValid(definitionsDirectory);

        var repository = TomlMapNodeRepositoryLoader.LoadFromDirectory(definitionsDirectory);

        Assert.NotEmpty(repository.All);
    }
}
