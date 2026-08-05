using Godot;
using System;
using System.Collections.Generic;

public abstract partial class Hotspot : Area2D
{		
	public Marker2D InteractionPoint { get; set; } = null!;
	
	public override void _Ready()
	{
		InteractionPoint = GetNode<Marker2D>("Marker2D");
		InputPickable = true;
	}
	
	public abstract string ExecuteAction(HotspotAction action);	
	
	public abstract IEnumerable<HotspotAction> GetAvailableActions();

	public virtual string GetActionLabel(HotspotAction action)
	{
		return  action.ToString();
	}
}
