using System;
using System.Collections.Generic;
using VikingJamGame.Models.GameEvents.Runtime;

namespace VikingJamGame.Repositories.GameEvents;

public sealed class InMemoryGameEventRepository : IGameEventRepository
{
    private readonly Dictionary<string, GameEvent> _eventsById;

    public InMemoryGameEventRepository(IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var map = new Dictionary<string, GameEvent>(StringComparer.Ordinal);
        foreach (var gameEvent in events)
        {
            if (!map.TryAdd(gameEvent.Id, gameEvent))
            {
                throw new InvalidOperationException(
                    $"Duplicate event id '{gameEvent.Id}' found while creating the repository.");
            }
        }

        _eventsById = map;
    }

    public IReadOnlyCollection<GameEvent> All => _eventsById.Values;

    public GameEvent GetById(string eventId)
    {
        if (TryGetById(eventId, out var gameEvent))
        {
            return gameEvent;
        }

        throw new KeyNotFoundException($"No game event found with id '{eventId}'.");
    }

    public bool TryGetById(string eventId, out GameEvent gameEvent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventId);

        return _eventsById.TryGetValue(eventId, out gameEvent!);
    }

    public bool TryGetNextEvent(GameEventOption selectedOption, out GameEvent nextEvent)
    {
        ArgumentNullException.ThrowIfNull(selectedOption);

        if (string.IsNullOrWhiteSpace(selectedOption.NextEventId))
        {
            nextEvent = null!;
            return false;
        }

        return _eventsById.TryGetValue(selectedOption.NextEventId, out nextEvent!);
    }
}
