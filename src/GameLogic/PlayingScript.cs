using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using VikingJamGame.GameLogic.Nodes;
using VikingJamGame.Models;

namespace VikingJamGame.GameLogic;

[GlobalClass]
[Meta(typeof(IAutoNode))]
public partial class PlayingScript : Node2D,
    IProvide<GameLoopMachine>, IProvide<GodotMapGenerator>, IProvide<GodotMapNodeRepository>,
    IProvide<ToolTipHandler>, IProvide<GodotNavigationSession>, IProvide<Camera2D>, IProvide<CameraController>,
    IProvide<GodotItemRepository>
{
    [Export] private GodotMapGenerator Generator { get; set; } = null!;
    [Export] private GodotMapLinkRenderer LinkRenderer { get; set; } = null!;
    [Export] private GodotNavigationSession NavigationSession { get; set; } = null!;
    [Export] private GodotMapNodeRepository NodeRepository { get; set; } = null!;
    [Export] private ToolTipHandler ToolTipHandler { get; set; } = null!;
    [Export] private Camera2D Camera { get; set; } = null!;
    [Export] private CameraController CameraController { get; set; } = null!;
    [Export] private GodotEventManager EventManager { get; set; } = null!;
    [Export] private GodotGameEventRepository EventRepository { get; set; } = null!;
    [Export] private GodotItemRepository ItemRepository { get; set; } = null!;

    [Dependency] private PlayerInfo PlayerInfo => this.DependOn<PlayerInfo>();
    [Dependency] private GameResources GameResources => this.DependOn<GameResources>();
    [Dependency] private GameStateMachine GameState => this.DependOn<GameStateMachine>();

    private readonly GameLoopMachine _machine = new();

    GameLoopMachine IProvide<GameLoopMachine>.Value() => _machine;
    GodotMapGenerator IProvide<GodotMapGenerator>.Value() => Generator;
    GodotMapNodeRepository IProvide<GodotMapNodeRepository>.Value() => NodeRepository;
    ToolTipHandler IProvide<ToolTipHandler>.Value() => ToolTipHandler;
    GodotNavigationSession IProvide<GodotNavigationSession>.Value() => NavigationSession;
    Camera2D IProvide<Camera2D>.Value() => Camera;
    CameraController IProvide<CameraController>.Value() => CameraController;
    GodotItemRepository IProvide<GodotItemRepository>.Value() => ItemRepository;

    public void OnResolved()
    {
        _machine.Set(Generator);
        _machine.Set(LinkRenderer);
        _machine.Set(NavigationSession);
        _machine.Set(NodeRepository);
        _machine.Set(CameraController);
        _machine.Set(EventManager);
        EventRepository.Reload();
        _machine.Set(EventRepository);
        ItemRepository.Reload();
        _machine.Set(ItemRepository);
        _machine.Set(PlayerInfo);
        _machine.Set(GameResources);
        _machine.Set(GameState);
        _machine.Start();
    }

    public override void _Ready() => this.Provide();
    public override void _Notification(int what) => this.Notify(what);
}