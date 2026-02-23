using VikingJamGame.Repositories.Items;

namespace VikingJamGame.Tests.Content.Repositories;

public sealed class ItemContentTests
{
    [Fact]
    public void ItemDefinitions_AreSyntacticallyValid_AndCanBeLoaded()
    {
        var definitionsDirectory = ContentTestProjectPaths.ResolvePathFromProjectRoot(
            TomlItemRepositoryLoader.DEFAULT_ITEMS_DIRECTORY);

        TomlContentAssertions.AssertAllTomlFilesAreSyntacticallyValid(definitionsDirectory);

        var repository = TomlItemRepositoryLoader.LoadFromDirectory(definitionsDirectory);

        Assert.NotEmpty(repository.All);
    }
}
