using Chickensoft.LogicBlocks;
using VikingJamGame.Models;

namespace VikingJamGame.GameLogic;

public partial class GameLoopMachine
{
    public abstract partial record State
    {
        public record LosingConditionCheckPhase: State
        {
            public LosingConditionCheckPhase()
            {
                this.OnEnter(() =>
                {
                    if (HasLost())
                    {
                        Get<GameStateMachine>().Input(new GameStateMachine.Input.TriggerGameOver());
                    }
                    else
                    {
                        To<PlanningPhase>();
                    }
                });
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