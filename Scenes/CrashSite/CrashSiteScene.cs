using Godot;
using System;

public partial class CrashSiteScene : SceneBase
{
	private SceneTransitionArea _toPowerOffline = null!;
	private PoliceAlien _policeAlien = null!;
	private ParkingTicket _parkingTicket = null!;
	private Marker2D _policeStart = null!;
	private Marker2D _policeExit = null!;
	
	private static readonly ThoughtLine[] IntroThoughts =
	{
		new(
			"Well... that landing could have gone better.",
			Duration: 1.5,
			PauseAfter: 1.0
		),
		new(
			"At least I'm still in one piece.",
			Duration: 1.5,
			PauseAfter: 1.0
		),
		new(
			"I should find out where I am.",
			Duration: 1.5,
			PauseAfter: 0.0
		)
	};


	protected override void OnRoomReady()
	{
		string spawnPointName = GameState.Instance.NextSpawnPoint;
		bool arrivedFromSpawnPoint = !string.IsNullOrWhiteSpace(spawnPointName)
			&& GetNodeOrNull<Marker2D>($"SpawnPoints/{spawnPointName}") != null;

		_toPowerOffline = GetNode<SceneTransitionArea>("Transitions/ToPowerOffline");
		_policeAlien = GetNode<PoliceAlien>("PoliceTicketSequence/PoliceAlien");
		_policeStart = GetNode<Marker2D>("PoliceTicketSequence/StartPosition");
		_policeExit = GetNode<Marker2D>("PoliceTicketSequence/ExitPosition");
		_parkingTicket = GetNode<ParkingTicket>("SpaceCraft/ParkingTicket");

		_toPowerOffline.TransitionRequested += OnPowerOfflineTransitionRequested;
		_policeAlien.SetPresent(false);
		_parkingTicket.SetAvailable(
			GameState.Instance.HasReceivedParkingTicket
			&& !GameState.Instance.HasTakenParkingTicket
		);
		
		StartIntroThoughts();

		if (arrivedFromSpawnPoint && !GameState.Instance.HasReceivedParkingTicket)
		{
			Callable.From(StartParkingTicketSequence).CallDeferred();
		}
	}

	private async void StartParkingTicketSequence()
	{
		GameState.Instance.HasReceivedParkingTicket = true;
		SetInteractionEnabled(false);

		try
		{
			_policeAlien.GlobalPosition = _policeStart.GlobalPosition;
			_policeAlien.SetPresent(true);
			Player.ShowThought("Hey, sir! What are you doing? Is that... a ticket?", 3.2);

			await _policeAlien.PlayOnceAsync(PoliceAlien.WriteTicketAnimation);
			await _policeAlien.PlayOnceAsync(PoliceAlien.PlaceTicketAnimation);

			_parkingTicket.SetAvailable(true);
			await _policeAlien.WalkToAsync(_policeExit.GlobalPosition);

			_policeAlien.SetPresent(false);
		}
		finally
		{
			if (IsInsideTree())
			{
				SetInteractionEnabled(true);
			}
		}
	}
	
	private void StartIntroThoughts()
	{
		bool arrivedFromAnotherScene = !string.IsNullOrWhiteSpace(GameState.Instance.NextSpawnPoint);

		if (arrivedFromAnotherScene)
		{
			return;
		}
		
		if (GameState.Instance.HasShownCrashSiteIntro)
		{
			return;
		}

		GameState.Instance.HasShownCrashSiteIntro = true;

		_ = Player.ShowThoughtSequenceAsync(IntroThoughts, initialDelay: 0.6);
	}
	
	private void OnPowerOfflineTransitionRequested()
	{
		GameState.Instance.NextSpawnPoint = "FromCrashSite";

		Callable.From(ChangeToPowerOffline)
			.CallDeferred();
	}

	private void ChangeToPowerOffline()
	{
		Error result = GetTree().ChangeSceneToFile(Consts.PowerOfflineScenePath);

		if (result != Error.Ok)
		{
			GD.PushError($"Could not change scene: {result}");
		}
	}
}
