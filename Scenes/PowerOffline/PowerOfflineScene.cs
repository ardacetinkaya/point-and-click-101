using Godot;

public partial class PowerOfflineScene : SceneBase
{
	
	private Sprite2D _backgroundLight = null!;
	private bool _powerRestored;
	
	private CableHotspot _cable = null!;
	private EntranceHotspot _entranceHotspot = null!;
	private ExitHotspot _exitHotspot = null!;
	private bool _exitPending;
	
	private SceneTransitionArea _toCrashSite = null!;

	protected override void OnRoomReady()
	{
		_cable = GetNode<CableHotspot>("Hotspots/Cable");
		_entranceHotspot = GetNode<EntranceHotspot>("Hotspots/Entrance");
		_exitHotspot = GetNode<ExitHotspot>("Hotspots/Exit");
		
		_toCrashSite = GetNode<SceneTransitionArea>("Transitions/ToCrashSite");

		_toCrashSite.TransitionRequested += OnCrashSiteTransitionRequested;
		
		_backgroundLight = GetNode<Sprite2D>("Background-LightsOn");
		_backgroundLight.Hide();
		
		_cable.CableConnected += GameState.Instance.RestoreStationPower;
		_entranceHotspot.EntryRequested += OnEntryRequested;
		_exitHotspot.ExitRequested += OnExitRequested;
		Player.DestinationReached += OnPlayerReachedExit;
		GameState.Instance.StationPowerChanged += OnStationPowerChanged;
		
		if (GameState.Instance.StationHasPower)
		{
			ShowRestoredPowerImmediately();
		}
	}
	
	public override void _ExitTree() => GameState.Instance.StationPowerChanged -= OnStationPowerChanged;

	
	private void RestorePower()
	{
		if (_powerRestored)
		{
			return;
		}

		_powerRestored = true;
		_backgroundLight.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		_backgroundLight.Show();

		Tween tween = CreateTween();

		tween.TweenProperty(_backgroundLight, "modulate:a", 1.0f, 0.4f);
	}
	
	private void ShowRestoredPowerImmediately()
	{
		_powerRestored = true;
		_backgroundLight.Modulate = new Color( 1.0f, 1.0f, 1.0f);
		_backgroundLight.Show();
	}
	
	private void OnPlayerReachedExit()
	{
		if (!_exitPending)
		{
			return;
		}

		_exitPending = false;

		if (!_exitHotspot.CanExit())
		{
			Player.ShowThought(_exitHotspot.BlockedThought, 3.5);

			return;
		}
		
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
	
	private void OnStationPowerChanged(bool hasPower)
	{
		if (hasPower)
		{
			RestorePower();
		}
		else
		{
			_powerRestored = false;
			_backgroundLight.Hide();
		}
	}
	
	
	private void OnEntryRequested()
	{
		Callable.From(ChangeToStationInterior)
			.CallDeferred();
	}

	private void OnExitRequested()
	{
		_exitPending = true;

		ActionMenu.HideMenu();

		Player.MoveTo(_exitHotspot.InteractionPoint.GlobalPosition);
	}

	private void ChangeToStationInterior()
	{
		Error result = GetTree().ChangeSceneToFile(Consts.StationInteriorScenePath);

		if (result != Error.Ok)
		{
			GD.PushError($"Could not change scene: {result}");
		}
	}

	
	private void OnCrashSiteTransitionRequested()
	{
		GameState.Instance.NextSpawnPoint = "FromPowerOffline";

		Callable.From(ChangeToCrashSite)
			.CallDeferred();
	}

	private void ChangeToCrashSite()
	{
		Error result = GetTree().ChangeSceneToFile(Consts.CrashSiteScenePath);

		if (result != Error.Ok)
		{
			GD.PushError($"Could not change scene: {result}");
		}
	}
}
