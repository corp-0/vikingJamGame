using Chickensoft.LogicBlocks;
using Godot;
using VikingJamGame.Models;

namespace VikingJamGame.GameLogic;

public partial class GameLoopMachine
{
    public abstract partial record State
    {
        public record LosingConditionCheckPhase: State, IGet<Input.LosingConditionEvaluated>
        {
            public LosingConditionCheckPhase()
            {
                this.OnEnter(() =>
                {
                    GD.Print("Losing Condition Check Phase");
                    Input(new Input.LosingConditionEvaluated());
                });
            }

            public Transition On(in Input.LosingConditionEvaluated input)
            {
                if (HasLost())
                {
                    Get<GameStateMachine>().Input(new GameStateMachine.Input.TriggerGameOver());
                    return ToSelf();
                }

                return To<PlanningPhase>();
            }
            
            private bool HasLost()
            {
                var gameResource = Get<GameResources>();
                var playerInfo = Get<PlayerInfo>();

                if (gameResource.Population <= 0) return true;
                if (gameResource.Food <= 0) return true;
                if (playerInfo.Strength <= 0) return true;
                if (playerInfo.Honor <= 0) return true;
            
                return false;
            }
        }
    }
}
