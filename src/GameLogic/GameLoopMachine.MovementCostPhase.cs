using Chickensoft.LogicBlocks;
using Godot;
using VikingJamGame.Models;

namespace VikingJamGame.GameLogic;

public partial class GameLoopMachine
{
    public abstract partial record State
    {
        public record MovementCostPhase : State, IGet<Input.MovementCostApplied>
        {
            public MovementCostPhase() => this.OnEnter(() =>
            {
                GD.Print("MovementCostPhase");
                var gameResources = Get<GameResources>();
                gameResources.ConsumeFoodForMovement(CalculateFoodConsumption());
                Input(new Input.MovementCostApplied());
            });

            public Transition On(in Input.MovementCostApplied input) => To<LosingConditionCheckPhase>();

            private int CalculateFoodConsumption()
            {
                var gameResources = Get<GameResources>();
                return (int)(gameResources.Population * 0.2);
            }
        }
    }
}
