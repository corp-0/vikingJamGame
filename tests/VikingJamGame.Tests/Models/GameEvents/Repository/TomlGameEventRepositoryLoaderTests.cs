using VikingJamGame.Repositories.GameEvents;
using VikingJamGame.Models.GameEvents.Compilation;
using VikingJamGame.TemplateUtils;
using VikingJamGame.Tests.TestDoubles;

namespace VikingJamGame.Tests.Models.GameEvents.Repository;

public sealed class TomlGameEventRepositoryLoaderTests
{
    [Fact]
    public void LoadFromDirectory_LoadsAllTomlFiles_AndResolvesChainLinks()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            WriteToml(
                tempDirectory,
                "start.toml",
                """
                Id = "event.start"
                Name = "Start"
                Description = "desc"

                [[OptionDefinitions]]
                DisplayText = "Continue"
                ResolutionText = "Continue"
                Order = 1
                NextEventId = "event.next"
                """);

            WriteToml(
                tempDirectory,
                "next.toml",
                """
                Id = "event.next"
                Name = "Next"
                Description = "desc"

                [[OptionDefinitions]]
                DisplayText = "End"
                ResolutionText = "End"
                Order = 1
                """);

            var repository = TomlGameEventRepositoryLoader.LoadFromDirectory(
                tempDirectory,
                new RecordingCommandRegistry());

            Assert.Equal(2, repository.All.Count);
            Assert.True(repository.TryGetById("event.start", out var start));
            Assert.True(repository.TryGetNextEvent(start.Options[0], out var next));
            Assert.Equal("event.next", next.Id);
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    [Fact]
    public void LoadFromDirectory_ThrowsWhenNextEventReferenceIsMissing()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            WriteToml(
                tempDirectory,
                "start.toml",
                """
                Id = "event.start"
                Name = "Start"
                Description = "desc"

                [[OptionDefinitions]]
                DisplayText = "Continue"
                ResolutionText = "Continue"
                Order = 1
                NextEventId = "event.missing"
                """);

            InvalidOperationException exception = Assert
                .Throws<InvalidOperationException>(() =>
                    TomlGameEventRepositoryLoader.LoadFromDirectory(
                        tempDirectory,
                        new RecordingCommandRegistry()));

            Assert.Contains("points to missing NextEventId 'event.missing'", exception.Message);
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    [Fact]
    public void LoadFromDirectory_ThrowsWhenRequiredFieldIsMissing()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            WriteToml(
                tempDirectory,
                "broken.toml",
                """
                Id = "event.broken"
                Description = "desc"
                """);

            InvalidOperationException exception = Assert
                .Throws<InvalidOperationException>(() =>
                    TomlGameEventRepositoryLoader.LoadFromDirectory(
                        tempDirectory,
                        new RecordingCommandRegistry()));

            Assert.Contains("missing required key 'Name'", exception.Message);
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    [Fact]
    public void LoadFromDirectory_RendersTemplatedTextWithProvidedContext()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            WriteToml(
                tempDirectory,
                "templated.toml",
                """
                Id = "event.templated"
                Name = "{Title}"
                Description = "{He} sees the horizon."

                [[OptionDefinitions]]
                DisplayText = "Join {him}"
                ResolutionText = "{His} banner rises."
                Order = 1
                """);

            var repository = TomlGameEventRepositoryLoader.LoadFromDirectory(
                tempDirectory,
                new RecordingCommandRegistry(),
                new GameEventTemplateContext(BirthChoice.Boy, "Bjorn", "Sea Wolf"));

            var gameEvent = repository.GetById("event.templated");
            var option = gameEvent.Options[0];

            Assert.Equal("Sea Wolf", gameEvent.Name);
            Assert.Equal("He sees the horizon.", gameEvent.Description);
            Assert.Equal("Join him", option.DisplayText);
            Assert.Equal("His banner rises.", option.ResolutionText);
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "VikingJamGame.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void WriteToml(string directoryPath, string fileName, string contents)
    {
        var filePath = Path.Combine(directoryPath, fileName);
        File.WriteAllText(filePath, contents);
    }

    private static void DeleteDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }
}
