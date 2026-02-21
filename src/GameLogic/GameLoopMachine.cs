using System.Collections.Generic;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Godot;
using VikingJamGame.GameLogic.Nodes;
using VikingJamGame.Models.GameEvents.Runtime;
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
        public readonly record struct EventResolved(EventResults Results);
    }

    public abstract partial record State : StateLogic<State>
    {
        private static string? ResolveConsecutiveVisitEventId(
            IReadOnlyDictionary<int, string> eventsByConsecutiveVisitCount,
            int consecutiveVisitCount)
        {
            if (eventsByConsecutiveVisitCount.TryGetValue(consecutiveVisitCount, out string? eventId)
                && !string.IsNullOrWhiteSpace(eventId))
            {
                return eventId;
            }

            return null;
        }


        /// <summary>
        /// Resolves the event ID for the current map node, checking consecutive-visit
        /// events first, then forced-first-event, then picking randomly from the pool.
        /// Returns null if no event applies.
        /// </summary>
        protected string? ResolveCurrentNodeEventId()
        {
            GodotNavigationSession nav = Get<GodotNavigationSession>();
            GodotMapNodeRepository nodeRepository = Get<GodotMapNodeRepository>();

            string currentNodeKind = nav.CurrentNode.Kind;
            if (!nodeRepository.TryGetByKind(currentNodeKind, out MapNodeDefinition nodeDefinition))
            {
                GD.PushWarning(
                    $"No map-node definition for kind '{currentNodeKind}' while resolving node event.");
                return null;
            }

            // 1. Consecutive-visit event takes priority
            string? eventId = ResolveConsecutiveVisitEventId(
                nodeDefinition.EventsOnConsecutiveVisits,
                nav.ConsecutiveNodesOfSameType);
            if (eventId is not null)
                return eventId;

            // 2. Forced first event
            if (nodeDefinition.ForcedFirstEvent is { } forced && !string.IsNullOrWhiteSpace(forced))
                return forced;

            // 3. Random pick from pool
            if (nodeDefinition.EventsPool.Count == 0)
                return null;

            int index = (int)(GD.Randi() % (uint)nodeDefinition.EventsPool.Count);
            return nodeDefinition.EventsPool[index];
        }

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

            if (map.EndNodeId is { } endNodeId)
            {
                visibleNodeIds.Add(endNodeId);
                knownIdentityNodeIds.Add(endNodeId);
            }

            generator.SetNodeVisibility(visibleNodeIds, knownIdentityNodeIds);
            generator.SetCurrentNode(nav.CurrentNodeId);
            linkRenderer.RenderConnectionsForCurrentNode(nav.CurrentNodeId, visibleNodeIds);
        }
    }

    public override Transition GetInitialState() => To<State.Initialization>();
}
