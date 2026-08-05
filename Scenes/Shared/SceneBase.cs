using Godot;
// using PointAndClick101.Scenes.Shared;
using System;

public abstract partial class SceneBase : Node2D
{
	protected Player Player { 
		get; 
		private set; 
	} = null!;
	
	protected ActionMenu ActionMenu {
		get;
		private set;
	} = null!;
	
	private Hotspot? _pendingHotspot;
	private HotspotAction _pendingAction;
	
	public override void _Ready()
	{
		Player = GetNode<Player>("Player");
		ActionMenu = GetNode<ActionMenu>("UI/ActionMenu");

		Player?.DestinationReached += OnPlayerDestinationReached;
		ActionMenu?.ActionChosen += OnActionChosen;

		OnRoomReady();
		PlacePlayer();
	}

	protected virtual void OnRoomReady()
	{
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mouseEvent)
		{
			return;
		}

		if (mouseEvent.ButtonIndex != MouseButton.Left || !mouseEvent.Pressed)
		{
			return;
		}
		
		Vector2 worldPosition = GetGlobalMousePosition();

		Hotspot? clickedHotspot = FindHotspotAt(worldPosition);

		if (clickedHotspot != null)
		{
			ActionMenu?.ShowFor(clickedHotspot, mouseEvent.Position);

			GetViewport().SetInputAsHandled();
			return;
		}

		ActionMenu?.HideMenu();
		_pendingHotspot = null;

		Player?.MoveTo(GetGlobalMousePosition());
	}

	private Hotspot? FindHotspotAt(Vector2 worldPosition)
	{
		var query = new PhysicsPointQueryParameters2D
		{
			Position = worldPosition,
			CollideWithAreas = true,
			CollideWithBodies = false
		};

		var results = GetWorld2D()
			.DirectSpaceState
			.IntersectPoint(query, 32);

		foreach (var result in results)
		{
			GodotObject collider = result["collider"].As<GodotObject>();

			if (collider is Hotspot hotspot)
			{
				return hotspot;
			}
		}

		return null;
	}

	private void OnPlayerDestinationReached()
	{
		GD.Print("Player reached destination");
		if (_pendingHotspot == null)
		{
			return;
		}

		Hotspot hotspot = _pendingHotspot;
		HotspotAction action = _pendingAction;

		string thought = hotspot.ExecuteAction(_pendingAction);
		
		GD.Print($"Thought result: {thought}");
		Player.ShowThought(thought);

		_pendingHotspot = null;
	}

	private void OnActionChosen(Hotspot hotspot, int actionValue)
	{    
		GD.Print($"SceneBase received action: {(HotspotAction)actionValue}");
		
		_pendingHotspot = hotspot;
		_pendingAction = (HotspotAction)actionValue;

		Player.MoveTo(hotspot.InteractionPoint.GlobalPosition);
	}
	
	private void PlacePlayer()
	{
		if (GameState.Instance.TryTakeLoadedPlayerPosition(out Vector2 loadedPosition))
		{
			Player.GlobalPosition = loadedPosition;
			Player.Stop();
			return;
		}

		string spawnPointName = GameState.Instance.NextSpawnPoint;

		if (string.IsNullOrWhiteSpace(spawnPointName))
		{
			return;
		}

		GameState.Instance.NextSpawnPoint = string.Empty;

		Marker2D? spawnPoint = GetNodeOrNull<Marker2D>($"SpawnPoints/{spawnPointName}");

		if (spawnPoint == null)
		{
			GD.PushWarning($"Spawn point not found: {spawnPointName}");

			return;
		}

		Player?.GlobalPosition = spawnPoint.GlobalPosition;

		Player?.Stop();
	}

}
