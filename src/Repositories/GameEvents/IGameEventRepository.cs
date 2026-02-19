using System.Collections.Generic;
using VikingJamGame.Models.GameEvents.Runtime;

namespace VikingJamGame.Repositories.GameEvents;

public interface IGameEventRepository
{
    IReadOnlyCollection<GameEvent> All { get; }

    GameEvent GetById(string eventId);

    bool TryGetById(string eventId, out GameEvent gameEvent);

    bool TryGetNextEvent(GameEventOption selectedOption, out GameEvent nextEvent);
}
