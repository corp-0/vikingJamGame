using System;
using System.Collections.Generic;
using VikingJamGame.Models.Items;

namespace VikingJamGame.Repositories.Items;

public sealed class InMemoryItemRepository : IItemRepository
{
    private readonly Dictionary<string, Item> _itemsById;

    public InMemoryItemRepository(IEnumerable<Item> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var map = new Dictionary<string, Item>(StringComparer.Ordinal);
        foreach (Item item in items)
        {
            if (!map.TryAdd(item.Id, item))
            {
                throw new InvalidOperationException(
                    $"Duplicate item id '{item.Id}' found while creating repository.");
            }
        }

        _itemsById = map;
    }

    public IReadOnlyCollection<Item> All => _itemsById.Values;

    public Item GetById(string itemId)
    {
        if (TryGetById(itemId, out Item item))
        {
            return item;
        }

        throw new KeyNotFoundException($"No item found with id '{itemId}'.");
    }

    public bool TryGetById(string itemId, out Item item)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);

        return _itemsById.TryGetValue(itemId, out item!);
    }
}
