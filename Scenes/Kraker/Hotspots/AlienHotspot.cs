using Godot;
using System.Collections.Generic;

public partial class AlienHotspot : Hotspot
{
    [Signal]
    public delegate void ConversationRequestedEventHandler();

    public override string ExecuteAction(HotspotAction action)
    {
        if (action == HotspotAction.Talk)
        {
            EmitSignal(SignalName.ConversationRequested);
        }

        return string.Empty;
    }

    public override IEnumerable<HotspotAction> GetAvailableActions() =>
        [HotspotAction.Talk];

    public override string GetActionLabel(HotspotAction action) =>
        action == HotspotAction.Talk ? "Talk to alien" : base.GetActionLabel(action);
}
