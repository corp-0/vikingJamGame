using System.Collections.Generic;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using VikingJamGame.Models;
using VikingJamGame.Models.GameEvents;
using VikingJamGame.Models.GameEvents.Effects;
using VikingJamGame.Models.GameEvents.Runtime;
using VikingJamGame.Models.GameEvents.Stats;

namespace VikingJamGame.GameLogic.Nodes;

[GlobalClass]
[Meta(typeof(IAutoNode))]
public partial class GodotEventManager : CanvasLayer
{
    [Dependency]
    private GameLoopMachine GameLoop => this.DependOn<GameLoopMachine>();
    [Dependency]
    private PlayerInfo PlayerInfo => this.DependOn<PlayerInfo>();
    [Dependency]
    private GameResources GameResources => this.DependOn<GameResources>();

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
    [Export] private Separator Separator { get; set; } = null!;
    [Export] private Label QuestionLabel { get; set; } = null!;
    [Export] private VBoxContainer OptionsContainer { get; set; } = null!;
    [Export] private VBoxContainer ResolutionContainer { get; set; } = null!;
    [Export] private VBoxContainer ResolutionEffectsContainer { get; set; } = null!;
    [Export] private Label ResolutionLabel { get; set; } = null!;
    [Export] private Button ContinueButton { get; set; } = null!;

    private readonly GameEventEvaluator _evaluator = new();
    private EventResults _currentResults = new();
    private GameEventOption? _selectedOption;

    public void OnResolved()
    {
        Visible = false;
        ContinueButton.Pressed += OnContinueButtonPressed;
    }

    public override void _ExitTree()
    {
        ContinueButton.Pressed -= OnContinueButtonPressed;
    }

    public void TriggerEvent(string eventId)
    {
        GodotGameEventRepository repo = GameLoop.Get<GodotGameEventRepository>();

        if (!repo.TryGetById(eventId, out GameEvent gameEvent))
        {
            GD.PushWarning($"Event '{eventId}' not found in repository. Auto-resolving.");
            GameLoop.Input(new GameLoopMachine.Input.EventResolved(new EventResults()));
            return;
        }

        _currentResults = new EventResults();
        ShowEvent(gameEvent);
    }

    private void ShowEvent(GameEvent gameEvent)
    {
        // Reset UI to option-selection mode
        Visible = true;
        ResolutionContainer.Visible = false;
        DescriptionContainer.Visible = true;
        Separator.Visible = true;
        QuestionLabel.Visible = true;

        EventTitleLabel.Text = gameEvent.Name;
        EventDescriptionLabel.Text = gameEvent.Description;

        // Dynamically adjust dialog width based on description length
        AdjustDialogWidth(gameEvent.Description.Length);

        // Clear old option buttons
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

            // Capture for lambda
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

        // For long descriptions, cap the description panel height so options stay visible
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

        // Clear old effect labels
        foreach (Node child in ResolutionEffectsContainer.GetChildren())
        {
            child.QueueFree();
        }

        // Show costs as negative effects
        foreach (StatAmount cost in _selectedOption!.Costs)
        {
            AddEffectLabel($"-{cost.Amount} {cost.Stat}", isPositive: false);
        }

        // Show effects from the option
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
            GodotGameEventRepository repo = GameLoop.Get<GodotGameEventRepository>();
            if (repo.TryGetById(nextEventId, out GameEvent nextEvent))
            {
                _selectedOption = null;
                ShowEvent(nextEvent);
                return;
            }

            GD.PushWarning($"Chain event '{nextEventId}' not found. Finishing event.");
        }

        Visible = false;
        GameLoop.Input(new GameLoopMachine.Input.EventResolved(_currentResults));
    }

    private GameEventContext BuildContext()
    {
        GodotNavigationSession nav = GameLoop.Get<GodotNavigationSession>();
        return new GameEventContext
        {
            PlayerInfo = PlayerInfo,
            GameResources = GameResources,
            ItemRepository = GameLoop.Get<GodotItemRepository>(),
            CurrentNodeKind = nav.CurrentNode.Kind
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

    public override void _Notification(int what) => this.Notify(what);

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible) return;

        // Consume mouse input that wasn't handled by the UI controls (like ScrollContainer).
        // This prevents scroll/drag from reaching the camera controller.
        if (@event is InputEventMouseButton or InputEventMouseMotion)
        {
            GetViewport().SetInputAsHandled();
        }
    }
}