using Godot;

public partial class MapLocationHotspot : Area2D
{
    [Export]
    public bool IsEnabled { get; set; } = true;

    [Signal]
    public delegate void SelectedEventHandler();

    public override void _Ready()
    {
        InputPickable = true;
    }

    public override void _InputEvent(Viewport viewport, InputEvent inputEvent, int shapeIndex)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (inputEvent is not InputEventMouseButton mouseEvent)
        {
            return;
        }

        if (mouseEvent.ButtonIndex != MouseButton.Left ||
            !mouseEvent.Pressed)
        {
            return;
        }

        viewport.SetInputAsHandled();

        EmitSignal(SignalName.Selected);
    }
}