using VikingJamGame.Models.GameEvents.Conditions;
using VikingJamGame.Models.GameEvents.Effects;
using VikingJamGame.Repositories.GameEvents;
using VikingJamGame.Repositories.Items;

namespace VikingJamGame.Tests.Content.Repositories;

public sealed class GameEventContentTests
{
    [Fact]
    public void EventDefinitions_AreSyntacticallyValid_AndCanBeLoaded()
    {
        var definitionsDirectory = ContentTestProjectPaths.ResolvePathFromProjectRoot(
            TomlGameEventRepositoryLoader.DEFAULT_EVENTS_DIRECTORY);

        TomlContentAssertions.AssertAllTomlFilesAreSyntacticallyValid(definitionsDirectory);

        var eventRepository = TomlGameEventRepositoryLoader.LoadFromDirectory(definitionsDirectory);
        var itemRepository = TomlItemRepositoryLoader.LoadFromDirectory(
            ContentTestProjectPaths.ResolvePathFromProjectRoot(
                TomlItemRepositoryLoader.DEFAULT_ITEMS_DIRECTORY));

        Assert.NotEmpty(eventRepository.All);
        AssertAllItemReferencesResolve(eventRepository, itemRepository);
    }

    private static void AssertAllItemReferencesResolve(
        IGameEventRepository eventRepository,
        IItemRepository itemRepository)
    {
        var integrityErrors = new List<string>();

        foreach (var gameEvent in eventRepository.All)
        {
            foreach (var option in gameEvent.Options)
            {
                foreach (var condition in option.VisibilityConditions.OfType<HasItemCondition>())
                {
                    if (!itemRepository.TryGetById(condition.ItemId, out _))
                    {
                        integrityErrors.Add(
                            $"Event '{gameEvent.Id}' option {option.Order} has unknown item in Conditions: '{condition.ItemId}'.");
                    }
                }

                foreach (var effect in option.Effects.OfType<GrantItemEffect>())
                {
                    if (!itemRepository.TryGetById(effect.ItemId, out _))
                    {
                        integrityErrors.Add(
                            $"Event '{gameEvent.Id}' option {option.Order} has unknown item in Effects: '{effect.ItemId}'.");
                    }
                }
            }
        }

        Assert.True(
            integrityErrors.Count == 0,
            $"Found event content item-reference errors:{Environment.NewLine}{string.Join(Environment.NewLine, integrityErrors)}");
    }
}
