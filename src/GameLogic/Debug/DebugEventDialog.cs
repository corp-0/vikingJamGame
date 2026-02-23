using System.Collections.Generic;
using Godot;
using VikingJamGame.GameLogic.Nodes;
using VikingJamGame.Models;
using VikingJamGame.Models.GameEvents;
using VikingJamGame.Models.GameEvents.Effects;
using VikingJamGame.Models.GameEvents.Runtime;
using VikingJamGame.Models.GameEvents.Stats;
using VikingJamGame.Repositories.Items;

namespace VikingJamGame.GameLogic.Debug;

/// <summary>
/// Standalone event dialog that replicates GodotEventManager's UI behavior
/// without any GameLoopMachine or DI dependency.
/// </summary>
[GlobalClass]
public partial class DebugEventDialog : CanvasLayer
{
    [ExportCategory("External dependencies")]
    [Export] private PackedScene EventOptionButton { get; set; } = null!;
    [Export] private LabelSettings PositiveEffectLabelSettings { get; set; } = null!;
    [Export] private LabelSettings NegativeEffectLabelSettings { get; set; } = null!;

    [ExportCategory("Internal dependencies")]
    [Export] private Panel Overlay { get; set; } = null!;
    [Export] private CenterContainer MainContainer { get; set; } = null!;
    [Export] private VBoxContainer DialogContainer { get; set; } = null!;
    [Export] private Label EventTitleLabel { get; set; } = null!;
    [Export] private Label EventDescriptionLabel { get; set; } = null!;
    [Export] private PanelContainer DescriptionContainer { get; set; } = null!;
    [Export] private HSeparator Separator { get; set; } = null!;
    [Export] private Label QuestionLabel { get; set; } = null!;
    [Export] private VBoxContainer OptionsContainer { get; set; } = null!;
    [Export] private VBoxContainer ResolutionContainer { get; set; } = null!;
    [Export] private VBoxContainer ResolutionEffectsContainer { get; set; } = null!;
    [Export] private Label ResolutionLabel { get; set; } = null!;
    [Export] private Button ContinueButton { get; set; } = null!;

    [Signal] public delegate void EventFinishedEventHandler(string eventId);

    private readonly GameEventEvaluator _evaluator = new();
    private EventResults _currentResults = new();
    private GameEventOption? _selectedOption;

    // Dependencies set by the tester script
    private PlayerInfo _playerInfo = null!;
    private GameResources _gameResources = null!;
    private GodotGameEventRepository _eventRepo = null!;
    private IItemRepository _itemRepo = null!;
    private string? _currentNodeKind;

    public void Initialize(
        PlayerInfo playerInfo,
        GameResources gameResources,
        GodotGameEventRepository eventRepo,
        IItemRepository itemRepo)
    {
        _playerInfo = playerInfo;
        _gameResources = gameResources;
        _eventRepo = eventRepo;
        _itemRepo = itemRepo;
    }

    public override void _Ready()
    {
        Visible = false;
        ContinueButton.Pressed += OnContinueButtonPressed;
    }

    public override void _ExitTree()
    {
        ContinueButton.Pressed -= OnContinueButtonPressed;
    }

    public void SetCurrentNodeKind(string? nodeKind)
    {
        _currentNodeKind = nodeKind;
    }

    public void TriggerEvent(string eventId)
    {
        if (!_eventRepo.TryGetById(eventId, out GameEvent gameEvent))
        {
            GD.PushWarning($"Event '{eventId}' not found in repository.");
            return;
        }

        _currentResults = new EventResults();
        ShowEvent(gameEvent);
    }

    private void ShowEvent(GameEvent gameEvent)
    {
        Visible = true;
        ResolutionContainer.Visible = false;
        DescriptionContainer.Visible = true;
        Separator.Visible = true;
        QuestionLabel.Visible = true;

        EventTitleLabel.Text = gameEvent.Name;
        EventDescriptionLabel.Text = gameEvent.Description;

        AdjustDialogWidth(gameEvent.Description.Length);

        foreach (Node child in OptionsContainer.GetChildren())
        {
            child.QueueFree();
        }

        GameEventContext context = BuildContext();

        foreach (GameEventOption option in gameEvent.Options)
        {
            if (!_evaluator.IsVisible(option, context))
                continue;

            GodotEventOptionButton btn = EventOptionButton.Instantiate<GodotEventOptionButton>();
            OptionsContainer.AddChild(btn);

            string? costText = option.DisplayCost && option.Costs.Count > 0
                ? FormatCosts(option.Costs)
                : null;
            btn.Initialize(option.DisplayText, costText);

            bool affordable = _evaluator.IsAffordable(option, context);
            if (!affordable)
            {
                btn.SetDisabled(true);
            }

            GameEventOption captured = option;
            btn.OnPressed(() => OnOptionChosen(captured));
        }
    }

    private const float MinDialogWidth = 480f;
    private const float MaxDialogWidth = 720f;
    private const int ShortDescriptionThreshold = 150;
    private const int LongDescriptionThreshold = 500;
    private const float MaxDescriptionHeight = 300f;

    private void AdjustDialogWidth(int descriptionLength)
    {
        float width;
        if (descriptionLength <= ShortDescriptionThreshold)
        {
            width = MinDialogWidth;
        }
        else if (descriptionLength >= LongDescriptionThreshold)
        {
            width = MaxDialogWidth;
        }
        else
        {
            float t = (float)(descriptionLength - ShortDescriptionThreshold)
                      / (LongDescriptionThreshold - ShortDescriptionThreshold);
            width = Mathf.Lerp(MinDialogWidth, MaxDialogWidth, t);
        }

        DialogContainer.CustomMinimumSize = new Vector2(width, 0);

        float descMinHeight = descriptionLength > ShortDescriptionThreshold ? MaxDescriptionHeight : 120f;
        DescriptionContainer.CustomMinimumSize = new Vector2(0, descMinHeight);
    }

    private void OnOptionChosen(GameEventOption option)
    {
        _selectedOption = option;
        _currentResults.Add(option);

        ShowEventResolution(option.ResolutionText);
    }

    private void ShowEventResolution(string resolutionText)
    {
        foreach (Node child in OptionsContainer.GetChildren())
        {
            child.QueueFree();
        }

        ResolutionContainer.Visible = true;
        DescriptionContainer.Visible = false;
        Separator.Visible = false;
        QuestionLabel.Visible = false;
        ResolutionLabel.Text = resolutionText;

        foreach (Node child in ResolutionEffectsContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach (StatAmount cost in _selectedOption!.Costs)
        {
            AddEffectLabel($"-{cost.Amount} {cost.Stat}", isPositive: false);
        }

        GameEventContext context = BuildContext();
        foreach (IGameEventEffect effect in _selectedOption.Effects)
        {
            string? displayText = effect.GetDisplayText(context);
            if (displayText is not null)
            {
                AddEffectLabel(displayText, effect.IsPositive);
            }
        }
    }

    private void OnContinueButtonPressed()
    {
        if (_selectedOption?.NextEventId is { } nextEventId)
        {
            if (_eventRepo.TryGetById(nextEventId, out GameEvent nextEvent))
            {
                _selectedOption = null;
                ShowEvent(nextEvent);
                return;
            }

            GD.PushWarning($"Chain event '{nextEventId}' not found. Finishing event.");
        }

        Visible = false;
        EmitSignal(SignalName.EventFinished, _currentResults.ResolvedOptions.Count > 0
            ? _currentResults.ResolvedOptions[0].DisplayText
            : "unknown");
    }

    private GameEventContext BuildContext()
    {
        return new GameEventContext
        {
            PlayerInfo = _playerInfo,
            GameResources = _gameResources,
            ItemRepository = _itemRepo,
            CurrentNodeKind = _currentNodeKind
        };
    }

    private void AddEffectLabel(string text, bool isPositive)
    {
        Label lbl = new()
        {
            Text = text,
            LabelSettings = isPositive ? PositiveEffectLabelSettings : NegativeEffectLabelSettings,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        ResolutionEffectsContainer.AddChild(lbl);
    }

    private static string FormatCosts(IReadOnlyList<StatAmount> costs)
    {
        System.Text.StringBuilder sb = new();
        foreach (StatAmount cost in costs)
        {
            if (sb.Length > 0) sb.Append("  ");
            sb.Append($"-{cost.Amount} {cost.Stat}");
        }
        return sb.ToString();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible) return;

        if (@event is InputEventMouseButton or InputEventMouseMotion)
        {
            GetViewport().SetInputAsHandled();
        }
    }
}
