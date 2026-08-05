using Godot;
using System;

public partial class CrashSiteScene : SceneBase
{
	private SceneTransitionArea _toPowerOffline = null!;
	
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
		_toPowerOffline = GetNode<SceneTransitionArea>("Transitions/ToPowerOffline");

		_toPowerOffline.TransitionRequested += OnPowerOfflineTransitionRequested;
		
		StartIntroThoughts();

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
