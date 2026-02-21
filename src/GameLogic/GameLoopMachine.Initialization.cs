using Chickensoft.LogicBlocks;
using Godot;
using VikingJamGame.GameLogic.Nodes;

namespace VikingJamGame.GameLogic;

public partial class GameLoopMachine
{
    public abstract partial record State
    {
        public record Initialization : State, IGet<Input.EventResolved>
        {
            public Initialization()
            {
                this.OnEnter(() =>
                {
                    GD.Print("Initialization");
                    UpdateVisibility();

                    CameraController camera = Get<CameraController>();
                    camera.IntroPanFinished += OnIntroPanFinished;
                });

                this.OnExit(() =>
                {
                    CameraController camera = Get<CameraController>();
                    camera.IntroPanFinished -= OnIntroPanFinished;
                });
            }

            private void OnIntroPanFinished()
            {
                var eventManager = Get<GodotEventManager>();
                string? eventId = ResolveCurrentNodeEventId();
                if (eventId != null) eventManager.TriggerEvent(eventId);
            }

            public Transition On(in Input.EventResolved input) => To<PlanningPhase>();
        }
    }
}
