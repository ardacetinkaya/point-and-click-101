using Godot;

public partial class KrakerScene : SceneBase
{
    private SceneTransitionArea _toMap = null!;

    protected override void OnRoomReady()
    {
        _toMap = GetNode<SceneTransitionArea>("Transitions/ToMap");

        _toMap.TransitionRequested += OnMapTransitionRequested;
    }

    private void OnMapTransitionRequested()
    {
        Callable.From(ChangeToNovaMap)
            .CallDeferred();
    }

    private void ChangeToNovaMap()
    {
        Error result = GetTree().ChangeSceneToFile(Consts.NovaMapScenePath);

        if (result != Error.Ok)
        {
            GD.PushError($"Could not change scene: {result}");
        }
    }
}