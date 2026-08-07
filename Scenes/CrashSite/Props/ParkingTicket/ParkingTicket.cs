using System.Collections.Generic;
using Godot;

public partial class ParkingTicket : Hotspot
{
	private CollisionShape2D _collisionShape = null!;

	public override void _Ready()
	{
		base._Ready();
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
	}

	public void SetAvailable(bool isAvailable)
	{
		Visible = isAvailable;
		_collisionShape.Disabled = !isAvailable;
	}

	public override IEnumerable<HotspotAction> GetAvailableActions() =>
		[HotspotAction.Look, HotspotAction.Touch];

	public override string GetActionLabel(HotspotAction action) => action switch
	{
		HotspotAction.Look => "Look at ticket",
		HotspotAction.Touch => "Take ticket",
		_ => base.GetActionLabel(action)
	};

	public override string ExecuteAction(HotspotAction action) => action switch
	{
		HotspotAction.Look => "What?! This is a parking ticket for parking in the wrong place. I crashed here! How is that even fair?",
		HotspotAction.Touch => TakeTicket(),
		_ => "There is nothing I can do with it."
	};

	private string TakeTicket()
	{
		if (GameState.Instance.HasTakenParkingTicket)
		{
			return "I already took the parking ticket.";
		}

		if (!InventoryManager.Instance.AddItem(InventoryItemIds.ParkingTicket))
		{
			return "I already have the parking ticket.";
		}

		GameState.Instance.HasTakenParkingTicket = true;
		QueueFree();

		return "I'm taking this ticket. I am definitely appealing it.";
	}
}
