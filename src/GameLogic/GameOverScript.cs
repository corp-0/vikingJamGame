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

    [Export] private Button RestartButton { get; set; } = null!;
    
    public override void _Notification(int what) => this.Notify(what);
}