using System.Collections.Generic;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Godot;
using VikingJamGame.GameLogic.Nodes;
using VikingJamGame.Models.Navigation;

namespace VikingJamGame.GameLogic;

[Meta, LogicBlock(typeof(State))]
public partial class GameLoopMachine : LogicBlock<GameLoopMachine.State>
{
    private const int VISIBILITY_RANGE = 2;
    private const int IDENTITY_REVEAL_RANGE = 1;

    public static class Input
    {
        public readonly record struct DestinationSelected(int NodeId);
        public readonly struct EventResolved;
        public readonly struct MovementCostDone;
    }

    public abstract record State : StateLogic<State>
    {
        protected void UpdateVisibility()
        {
            GodotMapGenerator generator = Get<GodotMapGenerator>();
            GodotNavigationSession nav = Get<GodotNavigationSession>();
            GodotMapLinkRenderer linkRenderer = Get<GodotMapLinkRenderer>();
            NavigationMap map = generator.CurrentMap!;

            HashSet<int> visibleNodeIds = map.GetNodesWithinDistance(nav.CurrentNodeId, VISIBILITY_RANGE);
            visibleNodeIds.UnionWith(nav.VisitedNodeIds);

            HashSet<int> knownIdentityNodeIds = map.GetNodesWithinDistance(nav.CurrentNodeId, IDENTITY_REVEAL_RANGE);
            knownIdentityNodeIds.UnionWith(nav.VisitedNodeIds);

            generator.SetNodeVisibility(visibleNodeIds, knownIdentityNodeIds);
            linkRenderer.RenderConnectionsBetweenVisibleNodes(visibleNodeIds);
        }

        public record EventPhase : State
        {
            public EventPhase() => this.OnEnter(() =>
            {
                GD.Print("EventPhase");
                UpdateVisibility();
                // TODO: trigger the event at current node
            });

            // we trigger the event at the current Node, player has to resolve it
            // and the resources amount is adjusted accordingly
        }

        public record PlanningPhase : State, IGet<Input.DestinationSelected>
        {
            public PlanningPhase() => this.OnEnter(() => GD.Print("PlanningPhase"));

            public Transition On(in Input.DestinationSelected input)
            {
                GodotNavigationSession nav = Get<GodotNavigationSession>();

                if (!nav.CanMoveTo(input.NodeId))
                {
                    GD.Print($"Cannot move to node {input.NodeId}");
                    return ToSelf();
                }

                nav.MoveTo(input.NodeId);
                return To<EventPhase>();
            }
        }

        public record MovementCostPhase : State, IGet<Input.MovementCostDone>
        {
            public MovementCostPhase() => this.OnEnter(() =>
            {
                GD.Print("MovementCostPhase");
                // TODO: consume movement cost (skip on first turn)
                Input(new Input.MovementCostDone());
            });

            public Transition On(in Input.MovementCostDone input) => To<PlanningPhase>();
        }
    }

    public override Transition GetInitialState() => To<State.EventPhase>();
}
