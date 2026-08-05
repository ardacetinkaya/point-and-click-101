using Godot;

public partial class DirectExit : Area2D
{
    [Signal]
    public delegate void ExitRequestedEventHandler();
    
    public Marker2D InteractionPoint { get; private set; } = null!;


    public virtual bool CanExit()=> true;
    
    public override void _Ready()
    {
        InputPickable = true;

        InteractionPoint = GetNode<Marker2D>("Marker2D");
    }

    public override void _InputEvent(Viewport viewport, InputEvent inputEvent, int shapeIndex)
    {
        if (inputEvent is not InputEventMouseButton mouseEvent)
        {
            return;
        }

        if (mouseEvent.ButtonIndex != MouseButton.Left || !mouseEvent.Pressed)
        {
            return;
        }

        viewport.SetInputAsHandled();
        
        EmitSignal(SignalName.ExitRequested);
    }
}