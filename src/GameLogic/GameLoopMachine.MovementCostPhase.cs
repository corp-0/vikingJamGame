using Chickensoft.LogicBlocks;
using Godot;
using VikingJamGame.Models;

namespace VikingJamGame.GameLogic;

public partial class GameLoopMachine
{
    public abstract partial record State
    {
        public record MovementCostPhase : State
        {
            public MovementCostPhase() => this.OnEnter(() =>
            {
                GD.Print("MovementCostPhase");
                var gameResources = Get<GameResources>();
                gameResources.ConsumeFoodForMovement(CalculateFoodConsumption());
                To<LosingConditionCheckPhase>();
            });

            private int CalculateFoodConsumption()
            {
                var gameResources = Get<GameResources>();
                return (int)(gameResources.Population * 0.2);
            }
        }
    }
}
