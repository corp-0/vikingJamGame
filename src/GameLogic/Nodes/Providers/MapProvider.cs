using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

namespace VikingJamGame.GameLogic.Nodes.Providers;


[GlobalClass]
[Meta(typeof(IProvider))]
public partial class MapProvider: Node, IProvide<GodotMapGenerator>, IProvide<GodotMapNodeRepository>
{
    public override void _Notification(int what) => this.Notify(what);
    
    
    [Export] private GodotMapGenerator _generator = null!;
    GodotMapGenerator IProvide<GodotMapGenerator>.Value() => _generator;

    [Export] private GodotMapNodeRepository _repo = null!;
    GodotMapNodeRepository IProvide<GodotMapNodeRepository>.Value() => _repo;
    
    public override void _Ready() => this.Provide();
}