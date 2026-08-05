using Godot;

// namespace PointAndClick101.Scenes.Shared;

public partial class ActionMenu : PanelContainer
{
	[ExportGroup("Action Icons")]
	[Export]
	public Texture2D LookIcon { get; set; } = null!;

	[Export]
	public Texture2D TouchIcon { get; set; } = null!;

	[Export]
	public Texture2D UseIcon { get; set; } = null!;

	[Export]
	public Texture2D MoveIcon { get; set; } = null!;

	[Export]
	public Texture2D TalkIcon { get; set; } = null!;
	
	[Signal]
	public delegate void ActionChosenEventHandler(Hotspot hotspot, int action);

	private GridContainer _actionGrid = null!;
	private Hotspot _currentHotspot = null!;

	public override void _Ready()
	{
		_actionGrid = GetNode<GridContainer>("MarginContainer/Content/ActionGrid");

		Hide();
	}

	public void ShowFor(Hotspot hotspot, Vector2 screenPosition)
	{
		_currentHotspot = hotspot;

		ClearMenu();

		foreach (HotspotAction action in hotspot.GetAvailableActions())
		{
			var capturedAction = action;

			var button = new Button
			{
				Text = string.Empty,
				Icon = GetActionIcon(action),
				CustomMinimumSize = new Vector2(70.0f, 70.0f),
				ExpandIcon = true,
				Flat = true,
				FocusMode = FocusModeEnum.None,
				IconAlignment = HorizontalAlignment.Center,
				VerticalIconAlignment = VerticalAlignment.Center,
				TooltipText = hotspot.GetActionLabel(action)
			};

			button.AddThemeConstantOverride("icon_max_width", 64);
			button.AddThemeConstantOverride("h_separation", 12);

			button.Pressed += () => ChooseAction(capturedAction);
			
			_actionGrid.AddChild(button);
		}

		Show();

		Callable
			.From(() => PlaceInsideViewport(screenPosition))
			.CallDeferred();
	}

	public void HideMenu()=> Hide();

	private void PlaceInsideViewport(Vector2 screenPosition)
	{
		const float cursorOffset = 12.0f;
		const float screenMargin = 12.0f;

		ResetSize();

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		Vector2 menuSize = Size;

		float x = screenPosition.X + cursorOffset;
		float y = screenPosition.Y + cursorOffset;

		if (x + menuSize.X + screenMargin > viewportSize.X)
		{
			x = screenPosition.X - menuSize.X - cursorOffset;
		}

		if (y + menuSize.Y + screenMargin > viewportSize.Y)
		{
			y = screenPosition.Y - menuSize.Y - cursorOffset;
		}

		float maximumX = Mathf.Max(screenMargin, viewportSize.X - menuSize.X - screenMargin);

		float maximumY = Mathf.Max(screenMargin, viewportSize.Y - menuSize.Y - screenMargin);

		Position = new Vector2(Mathf.Clamp(x, screenMargin, maximumX), Mathf.Clamp(y, screenMargin, maximumY));
	}

	private void ChooseAction(HotspotAction action)
	{
		GD.Print($"ActionMenu selected: {action}");
		EmitSignal(SignalName.ActionChosen, _currentHotspot, (int)action);

		HideMenu();
	}

	private Texture2D? GetActionIcon(HotspotAction action)
	{
		return action switch
		{
			HotspotAction.Look => LookIcon,
			HotspotAction.Touch => TouchIcon,
			HotspotAction.Use => UseIcon,
			HotspotAction.Enter => MoveIcon,
			HotspotAction.Talk => TalkIcon,
			_ => null!
		};
	}
	
	private void ClearMenu()
	{
		foreach (Node child in _actionGrid.GetChildren())
		{
			_actionGrid.RemoveChild(child);
			child.QueueFree();
		}
	}
}