using System;
using System.Collections.Generic;
using Godot;
using VikingJamGame.Models.Items;
using VikingJamGame.Repositories.Items;

namespace VikingJamGame.GameLogic.Nodes;

[GlobalClass]
public partial class GodotItemRepository : Node, IItemRepository
{
    [Export]
    public string EditorItemsResourceDirectory { get; set; } =
        ItemDirectoryResolver.EDITOR_ITEMS_RESOURCE_DIRECTORY;

    [Export]
    public string BuildItemsRelativeDirectory { get; set; } =
        ItemDirectoryResolver.BUILD_ITEMS_RELATIVE_DIRECTORY;

    private IItemRepository? _innerRepository;

    public IReadOnlyCollection<Item> All => RequireLoaded().All;

    public void Reload()
    {
        var itemsDirectory = ItemDirectoryResolver.ResolveForRuntime(
            EditorItemsResourceDirectory,
            BuildItemsRelativeDirectory);

        _innerRepository = TomlItemRepositoryLoader.LoadFromDirectory(itemsDirectory);
    }

    public void ReloadFromDirectory(string directoryPath)
    {
        _innerRepository = TomlItemRepositoryLoader.LoadFromDirectory(directoryPath);
    }

    public Item GetById(string itemId) => RequireLoaded().GetById(itemId);

    public bool TryGetById(string itemId, out Item item) =>
        RequireLoaded().TryGetById(itemId, out item);

    private IItemRepository RequireLoaded()
    {
        return _innerRepository ?? throw new InvalidOperationException(
            "ItemRepositoryNode is not loaded. Call Reload() first.");
    }
}
