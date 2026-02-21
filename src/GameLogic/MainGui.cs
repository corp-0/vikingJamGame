using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using VikingJamGame.GameLogic.Nodes;
using VikingJamGame.Models;
using VikingJamGame.Models.GameEvents;
using VikingJamGame.Models.Items;

namespace VikingJamGame.GameLogic;

[GlobalClass][Meta(typeof(IAutoNode))]
public partial class MainGui : CanvasLayer
{
    private const string ITEM_ART_PATH = "res://resources/items/";
    private static readonly Texture2D FallbackIcon = GD.Load<Texture2D>(ITEM_ART_PATH + "icon.svg");

    [Dependency] private GameResources GameResources => this.DependOn<GameResources>();
    [Dependency] private PlayerInfo PlayerInfo => this.DependOn<PlayerInfo>();
    [Dependency] private ToolTipHandler ToolTipHandler => this.DependOn<ToolTipHandler>();
    [Dependency] private GodotNavigationSession Navigation => this.DependOn<GodotNavigationSession>();
    [Dependency] private GodotMapGenerator MapGenerator => this.DependOn<GodotMapGenerator>();
    [Dependency] private GodotItemRepository ItemRepository => this.DependOn<GodotItemRepository>();

    [ExportGroup("Topbar")]
    [ExportCategory("Labels")]
    [Export] private Label PopulationLabel { get; set; } = null!;
    [Export] private Label FoodLabel { get; set; } = null!;
    [Export] private Label CoinLabel { get; set; } = null!;

    [ExportCategory("Containers for tooltips")]
    [Export] private HBoxContainer PopulationContainer { get; set; } = null!;
    [Export] private HBoxContainer FoodContainer { get; set; } = null!;
    [Export] private HBoxContainer CoinContainer { get; set; } = null!;

    [ExportGroup("Player info")]
    [ExportCategory("Labels")]
    [Export] private Label NameLabel { get; set; } = null!;
    [Export] private Label TitleLabel { get; set; } = null!;
    [ExportCategory("Containers")]
    [Export] private VBoxContainer NameContainer { get; set; } = null!;

    [ExportGroup("Stats")]
    [ExportCategory("Bars")]
    [Export] private ProgressBar StrBar { get; set; } = null!;
    [Export] private ProgressBar HonorBar { get; set; } = null!;
    [Export] private ProgressBar FeatsBar { get; set; } = null!;

    [ExportCategory("Value Labels")]
    [Export] private Label StrValue { get; set; } = null!;
    [Export] private Label HonorValue { get; set; } = null!;
    [Export] private Label FeatsValue { get; set; } = null!;

    [ExportGroup("Inventory")]
    [Export] private TextureRect Slot1 { get; set; } = null!;
    [Export] private TextureRect Slot2 { get; set; } = null!;
    [Export] private TextureRect Slot3 { get; set; } = null!;
    
    [ExportGroup("Supplies indicator")]
    [Export] private Label SuppliesLabel { get; set; } = null!;

    private bool _resolved;
    private TextureRect[] _slotIcons = [];

    private void OnNameClicked(InputEvent @event)
    {
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            return;
        }

        if (!MapGenerator.TryGetVisualNode(Navigation.CurrentNodeId, out Node2D visualNode))
        {
            return;
        }

        Camera2D camera = GetViewport().GetCamera2D();
        Tween tween = CreateTween();
        tween.TweenProperty(camera, "global_position",  visualNode.GlobalPosition, 1d)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Sine);
    }

    private Vector2 GetMousePositionWithOffset() =>
        new(GetViewport().GetMousePosition().X, GetViewport().GetMousePosition().Y + 50);

    private void ShowSuppliesCost(int amount)
    {
        SuppliesLabel.Visible = true;
        SuppliesLabel.Text = $"The clan runs on food. Travel spent {amount}.";
        SuppliesLabel.Modulate = SuppliesLabel.Modulate with { A = 0f };

        var tween = CreateTween();
        tween.TweenProperty(SuppliesLabel, "modulate:a", 1.0f, 0.3f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        tween.TweenInterval(1.0f);
        tween.TweenProperty(SuppliesLabel, "modulate:a", 0.0f, 0.5f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.In);
    }
    
    private void OnHoveringPop() => ToolTipHandler.SetToolTip("Population", GetMousePositionWithOffset());
    private void OnHoveringFood() => ToolTipHandler.SetToolTip("Food", GetMousePositionWithOffset());
    private void OnHoveringCoin() => ToolTipHandler.SetToolTip("Gold", GetMousePositionWithOffset());

    private void OnExitHover() => ToolTipHandler.ClearToolTip();

    private void UpdatedValues()
    {
        if (!_resolved) return;

        PopulationLabel.Text = GameResources.Population.ToString();
        FoodLabel.Text = GameResources.Food.ToString();
        CoinLabel.Text = GameResources.Gold.ToString();
        NameLabel.Text = PlayerInfo.Name;
        TitleLabel.Text = PlayerInfo.Title;

        StrBar.MaxValue = PlayerInfo.MaxStrength;
        StrBar.Value = PlayerInfo.Strength;
        HonorBar.MaxValue = PlayerInfo.MaxHonor;
        HonorBar.Value = PlayerInfo.Honor;
        FeatsBar.MaxValue = PlayerInfo.MaxFeats;
        FeatsBar.Value = PlayerInfo.Feats;
        StrValue.Text = $"{PlayerInfo.Strength}/{PlayerInfo.MaxStrength}";
        HonorValue.Text = $"{PlayerInfo.Honor}/{PlayerInfo.MaxHonor}";
        FeatsValue.Text = $"{PlayerInfo.Feats}/{PlayerInfo.MaxFeats}";

        UpdateInventorySlots();
    }

    private void UpdateInventorySlots()
    {
        for (int i = 0; i < Inventory.MAX_SLOTS; i++)
        {
            Item? item = PlayerInfo.Inventory.Slots[i];
            if (item is not null)
            {
                string artPath = ITEM_ART_PATH + item.Art;
                _slotIcons[i].Texture = ResourceLoader.Exists(artPath)
                    ? GD.Load<Texture2D>(artPath)
                    : FallbackIcon;
            }
            else
            {
                _slotIcons[i].Texture = null;
            }
        }
    }

    private GameEventContext BuildContext() => new()
    {
        PlayerInfo = PlayerInfo,
        GameResources = GameResources,
        ItemRepository = ItemRepository
    };

    private void OnSlotInput(int slotIndex, InputEvent @event)
    {
        if (@event is not InputEventMouseButton { Pressed: true } mouse) return;

        Item? item = PlayerInfo.Inventory.Slots[slotIndex];
        if (item is null) return;

        switch (mouse.ButtonIndex)
        {
            case MouseButton.Left:
                PlayerInfo.Inventory.UseItem(slotIndex, BuildContext());
                break;
            case MouseButton.Right:
                PlayerInfo.Inventory.RemoveItem(slotIndex, BuildContext());
                break;
        }
    }

    private void OnSlotHover(int slotIndex)
    {
        Item? item = PlayerInfo.Inventory.Slots[slotIndex];
        if (item is null) return;

        string tooltip = item.Description.Length > 0
            ? $"{item.Name}\n{item.Description}"
            : item.Name;
        ToolTipHandler.SetToolTip(tooltip, GetMousePositionWithOffset());
    }

    public void OnResolved()
    {
        _resolved = true;
        _slotIcons = [Slot1, Slot2, Slot3];
        UpdatedValues();

        GameResources.GameResourcesChanged += UpdatedValues;
        PlayerInfo.PlayerInfoChanged += UpdatedValues;
        PlayerInfo.Inventory.InventoryChanged += UpdatedValues;
        PopulationContainer.MouseEntered += OnHoveringPop;
        FoodContainer.MouseEntered += OnHoveringFood;
        CoinContainer.MouseEntered += OnHoveringCoin;
        PopulationContainer.MouseExited += OnExitHover;
        FoodContainer.MouseExited += OnExitHover;
        CoinContainer.MouseExited += OnExitHover;
        NameContainer.GuiInput += OnNameClicked;

        for (int i = 0; i < _slotIcons.Length; i++)
        {
            int slot = i;
            PanelContainer panel = _slotIcons[i].GetParent<PanelContainer>();
            panel.GuiInput += @event => OnSlotInput(slot, @event);
            panel.MouseEntered += () => OnSlotHover(slot);
            panel.MouseExited += OnExitHover;
        }
    }

    public override void _ExitTree()
    {
        GameResources.GameResourcesChanged -= UpdatedValues;
        PlayerInfo.PlayerInfoChanged -= UpdatedValues;
        PlayerInfo.Inventory.InventoryChanged -= UpdatedValues;
        PopulationContainer.MouseEntered -= OnHoveringPop;
        FoodContainer.MouseEntered -= OnHoveringFood;
        CoinContainer.MouseEntered -= OnHoveringCoin;
        PopulationContainer.MouseExited -= OnExitHover;
        FoodContainer.MouseExited -= OnExitHover;
        CoinContainer.MouseExited -= OnExitHover;
        NameContainer.GuiInput -= OnNameClicked;
    }

    public override void _Notification(int what) => this.Notify(what);
}
