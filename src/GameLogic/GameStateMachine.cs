using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

namespace VikingJamGame.GameLogic;

[Meta, LogicBlock(typeof(State))]
public partial class GameStateMachine: LogicBlock<GameStateMachine.State>
{
    public enum InitialState
    {
        Prologue,
        Playing,
        GameOver
    }

    private InitialState _initialState = InitialState.Prologue;

    public void SetInitialState(InitialState initialState) => _initialState = initialState;

    public override Transition GetInitialState() =>
        _initialState switch
        {
            InitialState.Playing => To<State.Playing>(),
            InitialState.GameOver => To<State.GameOver>(),
            _ => To<State.Prologue>()
        };
    
    public static class Input
    {
        public readonly record struct FinishPrologue;
        public readonly record struct TriggerGameOver;
        public readonly record struct Restart;
    }
    
    public abstract record State : StateLogic<State>
    {
        public record Prologue : State, IGet<Input.FinishPrologue>
        {
            public Transition On(in Input.FinishPrologue input) => To<Playing>();
        }

        public record Playing : State, IGet<Input.TriggerGameOver>
        {
            public Transition On(in Input.TriggerGameOver input) => To<GameOver>();
        }
        
        public record GameOver : State, IGet<Input.Restart>
        {
            public Transition On(in Input.Restart input) => To<Prologue>();
        }
    }
}
