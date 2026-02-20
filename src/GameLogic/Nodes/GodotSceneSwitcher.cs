using System;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

namespace VikingJamGame.GameLogic.Nodes;

[GlobalClass]
[Meta(typeof(IAutoNode))]
public partial class GodotSceneSwitcher : Node
{
    [Export] private PackedScene _prologueScene = null!;
    [Export] private PackedScene _gamingScene = null!;
    [Export] private PackedScene _gameOverScene = null!;

    [Dependency] private GameStateMachine GameStateMachine => this.DependOn<GameStateMachine>();

    private IDisposable? _stateBinding;
    private Node? _activeSceneNode;
    private bool _machineStarted;

    public void OnResolved()
    {
        SwitchForCurrentState(GameStateMachine.Value);

        _stateBinding?.Dispose();
        _stateBinding = GameStateMachine.Bind()
            .When((GameStateMachine.State.Prologue _) => SwitchToPackedScene(_prologueScene, nameof(_prologueScene)))
            .When((GameStateMachine.State.Playing _) => SwitchToPackedScene(_gamingScene, nameof(_gamingScene)))
            .When((GameStateMachine.State.GameOver _) => SwitchToPackedScene(_gameOverScene, nameof(_gameOverScene)));
    }

    public void FinishPrologue() => GameStateMachine.Input(new GameStateMachine.Input.FinishPrologue());
    public void TriggerGameOver() => GameStateMachine.Input(new GameStateMachine.Input.TriggerGameOver());
    public void Restart() => GameStateMachine.Input(new GameStateMachine.Input.Restart());

    public override void _ExitTree()
    {
        _stateBinding?.Dispose();
        _stateBinding = null;
    }

    public override void _Notification(int what) => this.Notify(what);

    private void SwitchForCurrentState(GameStateMachine.State state)
    {
        switch (state)
        {
            case GameStateMachine.State.Prologue:
                SwitchToPackedScene(_prologueScene, nameof(_prologueScene));
                break;
            case GameStateMachine.State.Playing:
                SwitchToPackedScene(_gamingScene, nameof(_gamingScene));
                break;
            case GameStateMachine.State.GameOver:
                SwitchToPackedScene(_gameOverScene, nameof(_gameOverScene));
                break;
        }
    }

    private void SwitchToPackedScene(PackedScene? packedScene, string exportName)
    {
        if (packedScene is null)
        {
            GD.PushWarning(
                $"{nameof(GodotSceneSwitcher)} cannot switch scene because '{exportName}' is not assigned.");
            return;
        }

        Node? host = GetParent();
        if (host is null)
        {
            GD.PushWarning($"{nameof(GodotSceneSwitcher)} cannot switch scene because parent node is null.");
            return;
        }

        if (_activeSceneNode is not null && IsInstanceValid(_activeSceneNode))
        {
            Node? previousParent = _activeSceneNode.GetParent();
            previousParent?.RemoveChild(_activeSceneNode);
            _activeSceneNode.QueueFree();
        }

        Node nextScene = packedScene.Instantiate<Node>();
        host.AddChild(nextScene);
        _activeSceneNode = nextScene;
    }
}
