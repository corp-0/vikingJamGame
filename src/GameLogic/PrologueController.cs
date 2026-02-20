using Godot;

namespace VikingJamGame.GameLogic;

[GlobalClass]
public partial class PrologueController: Control
{
    [Export] private PrologueScript PrologueScript { get; set; } = null!;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        SetProcessUnhandledInput(true);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true })
        {
            PrologueScript.AdvanceNarration();
            AcceptEvent();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Echo: false })
        {
            PrologueScript.AdvanceNarration();
        }
    }
}
