using Godot;
using System.Collections.Generic;

public partial class EntranceHotspot : Hotspot
{
	private bool CanLook { get; set; } = true;

	private bool CanEnter { get; set; } = true;
	
	private string LookText { get; set; } = "Now, I can see inside...It seems safe.";
	
	private string LookTextWhenNoPower { get; set; } = "Hope this is not because I forgot to pay my bill...";
	
	private string EnterText { get; set; } = "Hmmmm, it is not a good idea...Not because I am afraid of dark.";

	[Signal]
	public delegate void EntryRequestedEventHandler();

	
	public override string ExecuteAction(HotspotAction action) => action switch 
	{
		HotspotAction.Look => !GameState.Instance.StationHasPower ? LookTextWhenNoPower : LookText,
		HotspotAction.Enter => TryEnter(),
		_ => "There is nothing that I can do now"
	};
	


	public override IEnumerable<HotspotAction> GetAvailableActions()
	{
		if (CanLook)
		{
			yield return HotspotAction.Look;
		}

		if (CanEnter)
		{
			yield return HotspotAction.Enter;
		}
	}
	
	private string TryEnter()
	{
		if (!GameState.Instance.StationHasPower)
		{
			return EnterText;
		}

   		EmitSignal(SignalName.EntryRequested);
		return "Mamma, I am home...";
	}
}
