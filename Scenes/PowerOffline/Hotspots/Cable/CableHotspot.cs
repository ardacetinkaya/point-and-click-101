using System.Collections.Generic;
using Godot;

public partial class CableHotspot : Hotspot
{
	private bool CanLook { get; set; } = true;
	
	private bool CanTouch { get; set; } = true;
	
	private bool CanUse { get; set; } = true;

	[Export(PropertyHint.MultilineText)]
	public string LookText { get; set; } = "Hope this is not because I forgot to pay my bill...";

	[Export(PropertyHint.MultilineText)]
	public string TouchText { get; set; } = "Hmmmm, it is not a good idea...";

	[Signal]
	public delegate void CableConnectedEventHandler();

	private Node2D _electricalEffect = null!;
	
	public override void _Ready()
	{
		base._Ready();
		_electricalEffect =	GetNode<Node2D>("ElectricalEffect");
		
		UpdateElectricalEffect();
	}
	
	public override string ExecuteAction(HotspotAction action)
	{
		return action switch
		{
			HotspotAction.Look => GameState.Instance.StationHasPower ? "Like a new" : LookText,
			HotspotAction.Touch => GameState.Instance.StationHasPower ? "I don't want to touch" : TouchText,
			HotspotAction.Use => ConnectCable(),
			_ => "There is nothing that I can do now"
		};
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
		if (CanUse && !GameState.Instance.StationHasPower)
		{
			yield return HotspotAction.Use;
		}
	}	

	private string ConnectCable()
	{
		if (GameState.Instance.StationHasPower)
		{
			return "Already connected";
		}

		_electricalEffect.Hide();

		EmitSignal(SignalName.CableConnected);
		
		return "Let the party start..";
	}
	
	private void UpdateElectricalEffect()
	{
		if (GameState.Instance.StationHasPower)
		{
			_electricalEffect.Hide();
		}
		else
		{
			_electricalEffect.Show();
		}
	}
}
