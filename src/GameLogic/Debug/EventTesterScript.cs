using System;
using System.Collections.Generic;
using Godot;
using VikingJamGame.GameLogic.Nodes;
using VikingJamGame.Models;
using VikingJamGame.Models.GameEvents;
using VikingJamGame.Models.GameEvents.Runtime;
using VikingJamGame.Models.Items;
using VikingJamGame.TemplateUtils;

namespace VikingJamGame.GameLogic.Debug;

[GlobalClass]
public partial class EventTesterScript : Node2D
{
    [ExportCategory("Repositories")]
    [Export] private GodotGameEventRepository EventRepository { get; set; } = null!;
    [Export] private GodotItemRepository ItemRepository { get; set; } = null!;
    [Export] private GodotMapNodeRepository MapRepository { get; set; } = null!;

    [ExportCategory("Dialog")]
    [Export] private DebugEventDialog EventDialog { get; set; } = null!;

    [ExportCategory("Event List")]
    [Export] private ItemList EventListControl { get; set; } = null!;

    [ExportCategory("Stat Controls")]
    [Export] private SpinBox StrengthSpin { get; set; } = null!;
    [Export] private SpinBox MaxStrengthSpin { get; set; } = null!;
    [Export] private SpinBox HonorSpin { get; set; } = null!;
    [Export] private SpinBox MaxHonorSpin { get; set; } = null!;
    [Export] private SpinBox FeatsSpin { get; set; } = null!;
    [Export] private SpinBox MaxFeatsSpin { get; set; } = null!;
    [Export] private SpinBox PopulationSpin { get; set; } = null!;
    [Export] private SpinBox FoodSpin { get; set; } = null!;
    [Export] private SpinBox GoldSpin { get; set; } = null!;
    [Export] private OptionButton NodeKindDropdown { get; set; } = null!;
    [Export] private Button ReloadButton { get; set; } = null!;

    [ExportCategory("Inventory Controls")]
    [Export] private OptionButton ItemDropdown { get; set; } = null!;
    [Export] private Button GrantItemButton { get; set; } = null!;
    [Export] private Button ClearInventoryButton { get; set; } = null!;
    [Export] private Label InventoryLabel { get; set; } = null!;

    [ExportCategory("Error Display")]
    [Export] private RichTextLabel ErrorLabel { get; set; } = null!;

    private readonly PlayerInfo _playerInfo = new();
    private readonly GameResources _gameResources = new();
    private readonly List<GameEvent> _loadedEvents = [];

    public override void _Ready()
    {
        EventDialog.Initialize(_playerInfo, _gameResources, EventRepository, ItemRepository);
        EventDialog.EventFinished += OnEventFinished;

        SetDefaultStats();
        ConnectStatControls();

        EventListControl.ItemActivated += OnEventItemActivated;
        ReloadButton.Pressed += OnReloadPressed;
        GrantItemButton.Pressed += OnGrantItemPressed;
        ClearInventoryButton.Pressed += OnClearInventoryPressed;

        ReloadDefinitions();
        RefreshInventoryDisplay();
    }

    public override void _ExitTree()
    {
        EventDialog.EventFinished -= OnEventFinished;
        EventListControl.ItemActivated -= OnEventItemActivated;
        ReloadButton.Pressed -= OnReloadPressed;
        GrantItemButton.Pressed -= OnGrantItemPressed;
        ClearInventoryButton.Pressed -= OnClearInventoryPressed;
        DisconnectStatControls();
    }

    private void SetDefaultStats()
    {
        _playerInfo.SetInitialInfo(
            name: "Test Viking",
            birthChoice: BirthChoice.Boy,
            title: "Jarl",
            strength: 10, maxStrength: 20,
            honor: 10, maxHonor: 20,
            feats: 0, maxFeats: 10);

        _gameResources.SetInitialResources(population: 50, food: 100, gold: 50);

        SyncSpinBoxesFromState();
    }

    private void PopulateEventList()
    {
        EventListControl.Clear();
        _loadedEvents.Clear();

        foreach (GameEvent evt in EventRepository.All)
        {
            _loadedEvents.Add(evt);
            EventListControl.AddItem($"[{evt.Id}] {evt.Name}");
        }
    }

    private void OnReloadPressed() => ReloadDefinitions();

    private void ReloadDefinitions()
    {
        ClearErrors();

        try
        {
            ItemRepository.Reload();
            PopulateItemDropdown();
        }
        catch (Exception ex)
        {
            ShowError("Item definitions", ex.Message);
        }

        try
        {
            MapRepository.Reload();
            PopulateNodeKindDropdown();
        }
        catch (Exception ex)
        {
            ShowError("Map node definitions", ex.Message);
        }

        try
        {
            EventRepository.Reload();
            PopulateEventList();
            GD.Print($"[EventTester] Reloaded: {_loadedEvents.Count} events loaded.");
        }
        catch (Exception ex)
        {
            ShowError("Event definitions", ex.Message);
            EventListControl.Clear();
            _loadedEvents.Clear();
        }
    }

    private void ShowError(string source, string message)
    {
        ErrorLabel.Visible = true;
        ErrorLabel.Text += $"[color=red][b]{source}:[/b][/color]\n{message}\n\n";
        GD.PushError($"[EventTester] {source}: {message}");
    }

    private void ClearErrors()
    {
        ErrorLabel.Text = "";
        ErrorLabel.Visible = false;
    }

    private void PopulateNodeKindDropdown()
    {
        NodeKindDropdown.Clear();
        NodeKindDropdown.AddItem("(none)");
        foreach (var node in MapRepository.All)
        {
            NodeKindDropdown.AddItem($"{node.Kind}");
        }
    }

    private string? GetSelectedNodeKind()
    {
        int selected = NodeKindDropdown.Selected;
        if (selected <= 0) return null;
        return NodeKindDropdown.GetItemText(selected);
    }

    private void PopulateItemDropdown()
    {
        ItemDropdown.Clear();
        foreach (Item item in ItemRepository.All)
        {
            ItemDropdown.AddItem($"[{item.Id}] {item.Name}");
        }
    }

    private void OnGrantItemPressed()
    {
        int selected = ItemDropdown.Selected;
        if (selected < 0) return;

        var allItems = new List<Item>(ItemRepository.All);
        if (selected >= allItems.Count) return;

        Item item = allItems[selected];

        if (_playerInfo.Inventory.IsFull)
        {
            GD.Print("[EventTester] Inventory full, cannot grant item.");
            return;
        }

        ApplyStatsFromSpinBoxes();
        _playerInfo.Inventory.AddItem(item, BuildContext());
        SyncSpinBoxesFromState();
        RefreshInventoryDisplay();
    }

    private void OnClearInventoryPressed()
    {
        for (int i = 0; i < Inventory.MAX_SLOTS; i++)
        {
            _playerInfo.Inventory.Slots[i] = null;
        }

        RefreshInventoryDisplay();
    }

    private void RefreshInventoryDisplay()
    {
        var parts = new List<string>();
        for (int i = 0; i < Inventory.MAX_SLOTS; i++)
        {
            Item? item = _playerInfo.Inventory.Slots[i];
            parts.Add(item is not null ? item.Name : "(empty)");
        }

        InventoryLabel.Text = $"Inventory: {string.Join(" | ", parts)}";
    }

    private GameEventContext BuildContext()
    {
        return new GameEventContext
        {
            PlayerInfo = _playerInfo,
            GameResources = _gameResources,
            ItemRepository = ItemRepository,
            CurrentNodeKind = GetSelectedNodeKind()
        };
    }

    private void OnEventItemActivated(long index)
    {
        if (index < 0 || index >= _loadedEvents.Count) return;
        int idx = (int)index;

        ApplyStatsFromSpinBoxes();
        EventDialog.SetCurrentNodeKind(GetSelectedNodeKind());
        EventDialog.TriggerEvent(_loadedEvents[idx].Id);
    }

    private void OnEventFinished(string eventId)
    {
        SyncSpinBoxesFromState();
        RefreshInventoryDisplay();
    }

    private void SyncSpinBoxesFromState()
    {
        _updatingFromCode = true;

        StrengthSpin.Value = _playerInfo.Strength;
        MaxStrengthSpin.Value = _playerInfo.MaxStrength;
        HonorSpin.Value = _playerInfo.Honor;
        MaxHonorSpin.Value = _playerInfo.MaxHonor;
        FeatsSpin.Value = _playerInfo.Feats;
        MaxFeatsSpin.Value = _playerInfo.MaxFeats;
        PopulationSpin.Value = _gameResources.Population;
        FoodSpin.Value = _gameResources.Food;
        GoldSpin.Value = _gameResources.Gold;

        _updatingFromCode = false;
    }

    private void ApplyStatsFromSpinBoxes()
    {
        _playerInfo.SetInitialInfo(
            name: "Test Viking",
            birthChoice: BirthChoice.Boy,
            title: "Jarl",
            strength: (int)StrengthSpin.Value, maxStrength: (int)MaxStrengthSpin.Value,
            honor: (int)HonorSpin.Value, maxHonor: (int)MaxHonorSpin.Value,
            feats: (int)FeatsSpin.Value, maxFeats: (int)MaxFeatsSpin.Value);

        _gameResources.SetInitialResources(
            population: (int)PopulationSpin.Value,
            food: (int)FoodSpin.Value,
            gold: (int)GoldSpin.Value);
    }

    // Prevent feedback loops when syncing spinboxes
    private bool _updatingFromCode;

    private void ConnectStatControls()
    {
        StrengthSpin.ValueChanged += OnStatSpinChanged;
        MaxStrengthSpin.ValueChanged += OnStatSpinChanged;
        HonorSpin.ValueChanged += OnStatSpinChanged;
        MaxHonorSpin.ValueChanged += OnStatSpinChanged;
        FeatsSpin.ValueChanged += OnStatSpinChanged;
        MaxFeatsSpin.ValueChanged += OnStatSpinChanged;
        PopulationSpin.ValueChanged += OnStatSpinChanged;
        FoodSpin.ValueChanged += OnStatSpinChanged;
        GoldSpin.ValueChanged += OnStatSpinChanged;
    }

    private void DisconnectStatControls()
    {
        StrengthSpin.ValueChanged -= OnStatSpinChanged;
        MaxStrengthSpin.ValueChanged -= OnStatSpinChanged;
        HonorSpin.ValueChanged -= OnStatSpinChanged;
        MaxHonorSpin.ValueChanged -= OnStatSpinChanged;
        FeatsSpin.ValueChanged -= OnStatSpinChanged;
        MaxFeatsSpin.ValueChanged -= OnStatSpinChanged;
        PopulationSpin.ValueChanged -= OnStatSpinChanged;
        FoodSpin.ValueChanged -= OnStatSpinChanged;
        GoldSpin.ValueChanged -= OnStatSpinChanged;
    }

    private void OnStatSpinChanged(double _)
    {
        if (_updatingFromCode) return;
        ApplyStatsFromSpinBoxes();
    }
}