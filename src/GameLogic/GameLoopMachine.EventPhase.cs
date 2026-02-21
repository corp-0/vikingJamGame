using Chickensoft.LogicBlocks;
using Godot;
using VikingJamGame.GameLogic.Nodes;
using VikingJamGame.Models;
using VikingJamGame.Models.GameEvents;
using VikingJamGame.Models.GameEvents.Runtime;

namespace VikingJamGame.GameLogic;

public partial class GameLoopMachine
{
    public abstract partial record State
    {
        public record EventPhase : State, IGet<Input.EventResolved>
        {
            public EventPhase() => this.OnEnter(() =>
            {
                GD.Print("EventPhase");
                UpdateVisibility();

                string? eventId = ResolveCurrentNodeEventId();
                if (eventId is not null)
                {
                    GD.Print($"Triggering event '{eventId}' at node '{Get<GodotNavigationSession>().CurrentNode.Kind}'.");
                    Get<GodotEventManager>().TriggerEvent(eventId);
                    return;
                }

                GD.Print("No event to trigger. Auto-resolving.");
                Input(new Input.EventResolved(new EventResults()));
            });

            public Transition On(in Input.EventResolved input)
            {
                // Apply accumulated event effects
                GameEventContext context = BuildEventContext();
                GameEventEvaluator evaluator = new();
                evaluator.ApplyAll(input.Results, context);

                return To<MovementCostPhase>();
            }

            private GameEventContext BuildEventContext()
            {
                GodotNavigationSession nav = Get<GodotNavigationSession>();
                return new GameEventContext
                {
                    PlayerInfo = Get<PlayerInfo>(),
                    GameResources = Get<GameResources>(),
                    ItemRepository = Get<GodotItemRepository>(),
                    CurrentNodeKind = nav.CurrentNode.Kind
                };
            }
        }
    }
}
