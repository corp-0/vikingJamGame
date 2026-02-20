using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using VikingJamGame.Models;

namespace VikingJamGame.GameLogic;

[GlobalClass][Meta(typeof(IAutoNode))]
public partial class MainGui: CanvasLayer
{
    [Dependency] private GameResources GameResources => this.DependOn<GameResources>();
    [Dependency] private PlayerInfo PlayerInfo => this.DependOn<PlayerInfo>();
    
    [Export] private Label PopulationLabel { get; set; } = null!;
    [Export] private Label FoodLabel { get; set; } = null!;
    [Export] private Label CoinLabel { get; set; } = null!;

    private bool _resolved;
    
    private void UpdatedValues()
    {
        if (!_resolved) return;

        PopulationLabel.Text = GameResources.Population.ToString();
        FoodLabel.Text = GameResources.Food.ToString();
        CoinLabel.Text = GameResources.Gold.ToString();
    }

    public void OnResolved()
    {
        _resolved = true;
        UpdatedValues();
        GameResources.GameResourcesChanged +=  UpdatedValues;
        PlayerInfo.PlayerInfoChanged +=  UpdatedValues;
    }
    public override void _Notification(int what) => this.Notify(what);
}