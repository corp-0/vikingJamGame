using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using VikingJamGame.GameLogic.Nodes;

namespace VikingJamGame.GameLogic;

[GlobalClass]
[Meta(typeof(IProvider))]
public partial class PlayingScript : Node2D,
    IProvide<GameLoopMachine>
{
    [Export] private GodotMapGenerator Generator { get; set; } = null!;
    [Export] private GodotMapLinkRenderer LinkRenderer { get; set; } = null!;
    [Export] private GodotNavigationSession NavigationSession { get; set; } = null!;

    private readonly GameLoopMachine _machine = new();

    GameLoopMachine IProvide<GameLoopMachine>.Value() => _machine;

    public override void _Ready()
    {
        _machine.Set(Generator);
        _machine.Set(LinkRenderer);
        _machine.Set(NavigationSession);
        _machine.Start();
        this.Provide();
    }

    public override void _Notification(int what) => this.Notify(what);
}