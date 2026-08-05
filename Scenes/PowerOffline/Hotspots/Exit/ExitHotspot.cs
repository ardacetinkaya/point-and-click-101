using Godot;

public partial class ExitHotspot : DirectExit
{
    [Export(PropertyHint.MultilineText)]
    public string BlockedThought { get; set; } =
        "I have no idea where to go. I should find a map first.";

    public override bool CanExit()
    {
        return GameState.Instance.HasTakenMap;
    }
}