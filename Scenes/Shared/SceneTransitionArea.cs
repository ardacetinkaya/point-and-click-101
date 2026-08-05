using Godot;

public partial class SceneTransitionArea : Area2D
{
    [Signal]
    public delegate void TransitionRequestedEventHandler();

    private bool _transitionRequested;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_transitionRequested)
        {
            return;
        }

        if (body is not Player)
        {
            return;
        }
        GD.Print("Player reached transition area");
        _transitionRequested = true;

        EmitSignal(SignalName.TransitionRequested);
    }
}