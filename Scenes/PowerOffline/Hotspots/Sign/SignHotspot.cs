using Godot;
using System.Collections.Generic;


public partial class SignHotspot : Hotspot
{
	[Export]
	public bool CanLook { get; set; } = true;

	[Export]
	public bool CanTouch { get; set; }

	[Export(PropertyHint.MultilineText)]
	public string LookTextWhenNoPower { get; set; } = "\"Power Offline\"!!! Is this the name of the place or just because of power...";

	[Export(PropertyHint.MultilineText)]
	public string LookTextWhenPower { get; set; } = "\"Power Offline\"!!! Still...What a name...";

	[Export(PropertyHint.MultilineText)]
	public string TouchText { get; set; } = "Hmmmm, a real sign...";

	public override void _Ready()
	{
		InteractionPoint = GetNode<Marker2D>("Marker2D");
		InputPickable = true;
	}

	public override IEnumerable<HotspotAction> GetAvailableActions()
	{
		if (CanLook)
		{
			yield return HotspotAction.Look;
		}

		if (CanTouch)
		{
			yield return HotspotAction.Touch;
		}
	}

	public override string GetActionLabel(HotspotAction action)
	{
		return action switch
		{
			HotspotAction.Look => "Look",
			HotspotAction.Touch => "Touch",
			_ => action.ToString()
		};
	}
	
	public override string ExecuteAction(HotspotAction action)
	{
		return action switch
		{
			HotspotAction.Look => !GameState.Instance.StationHasPower ? LookTextWhenNoPower : LookTextWhenPower,
			HotspotAction.Touch => TouchText,
			_ => "There is nothing that I can do now"
		};
	}	
}
