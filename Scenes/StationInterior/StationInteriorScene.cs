using Godot;

public partial class StationInteriorScene : SceneBase
{
	private DirectExit _exitToTerraNova = null!;
	private bool _exitPending;

	protected override void OnRoomReady()
	{
		_exitToTerraNova = GetNode<DirectExit>("Exit/PowerOffline");
		_exitToTerraNova.ExitRequested += OnExitRequested;
		
		Player.DestinationReached += OnPlayerReachedExit;
	}

	private void OnExitRequested()
	{
		_exitPending = true;

		ActionMenu.HideMenu();

		Player.MoveTo(_exitToTerraNova.InteractionPoint.GlobalPosition);
	}
	
	private void OnPlayerReachedExit()
	{
		if (!_exitPending)
		{
			return;
		}

		_exitPending = false;

		Callable.From(ChangeToTerraNova)
			.CallDeferred();
	}

	private void ChangeToTerraNova()
	{
		GameState.Instance.NextSpawnPoint = "FromStationInterior";
		
		Error result = GetTree().ChangeSceneToFile(Consts.TerraNovaScenePath);

		if (result != Error.Ok)
		{
			GD.PushError($"Could not change scene: {result}");
		}
	}
}
