using System.Collections.Generic;
using VikingJamGame.Models.Items;

namespace VikingJamGame.Repositories.Items;

public interface IItemRepository
{
    IReadOnlyCollection<Item> All { get; }

    Item GetById(string itemId);

    bool TryGetById(string itemId, out Item item);
}
