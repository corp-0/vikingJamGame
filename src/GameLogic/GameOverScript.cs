using System.Collections.Generic;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using VikingJamGame.Models;

namespace VikingJamGame.GameLogic;

[GlobalClass][Meta(typeof(IAutoNode))]
public partial class GameOverScript: Node2D
{
    [Dependency] private GameResources GameResources => this.DependOn<GameResources>();
    [Dependency] private PlayerInfo PlayerInfo => this.DependOn<PlayerInfo>();
    [Dependency] private GameStateMachine GameState => this.DependOn<GameStateMachine>();

    [Export] private Button RestartButton { get; set; } = null!;
    [Export] private VBoxContainer DefeatContainer { get; set; } = null!;
    [Export] private VBoxContainer VictoryContainer { get; set; } = null!;
    [Export] private VBoxContainer DefeatReasonsContainer { get; set; } = null!;
    [Export] private Label TitleLabel { get; set; } = null!;
    [Export] private LabelSettings ReasonsStyle { get; set; } = null!;
    
    [ExportGroup("Stats labels")]
    [Export] private Label Strength { get; set; } = null!;
    [Export] private Label Honor { get; set; } = null!;
    [Export] private Label Feats { get; set; } = null!;
    [Export] private Label Population { get; set; } = null!;
    [Export] private Label Food { get; set; } = null!;
    [Export] private Label Gold { get; set; } = null!;

    private List<string> DefeatReasons { get; set; } = [];
    private bool IsVictory => DefeatReasons.Count == 0;

    private void BuildReaons()
    {
        if (GameResources.Population <= 0)
        {
            DefeatReasons.Add("- You lost all your people");
        }
        
        if (GameResources.Food <= 0)
        {
            DefeatReasons.Add("- You had no more food to continue");
        }
        
        if (PlayerInfo.Strength <= 0)
        {
            DefeatReasons.Add("- You died in battle");
        }

        if (PlayerInfo.Honor <= 0)
        {
            DefeatReasons.Add("- You were dishonored");
        }
    }
    
    private void SetStatValues() 
    {
        Strength.Text = PlayerInfo.Strength.ToString();
        Honor.Text = PlayerInfo.Honor.ToString();
        Feats.Text = PlayerInfo.Feats.ToString();
        Population.Text = GameResources.Population.ToString();
        Food.Text = GameResources.Food.ToString();
        Gold.Text = GameResources.Gold.ToString();
    }
    
    
    private void BuildScreen()
    {
        BuildReaons();
        DefeatContainer.Visible = !IsVictory;
        VictoryContainer.Visible = IsVictory;
        TitleLabel.Text = IsVictory ? "VICTORY" : "DEFEAT";
        if (!IsVictory)
        {
            foreach (string reason in DefeatReasons)
            {
                Label label = new();
                label.Text = reason;
                label.LabelSettings = ReasonsStyle;
                DefeatReasonsContainer.AddChild(label);
            }
        }
        
        SetStatValues();
    }

    public void OnResolved()
    {
        RestartButton.Pressed +=  OnRestartClicked;
        BuildScreen();
    }

    public override void _ExitTree()
    {
        RestartButton.Pressed -= OnRestartClicked;
    }

    private void OnRestartClicked() => GameState.Input(new GameStateMachine.Input.Restart());
    public override void _Notification(int what) => this.Notify(what);
}