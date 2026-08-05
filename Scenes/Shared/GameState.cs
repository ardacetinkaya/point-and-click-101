using Godot;

public partial class GameState : Node
{
	public static GameState Instance { get; private set; } = null!;
	
	public string NextSpawnPoint { get; set; } = string.Empty;
	public bool HasLookedAtMap { get; set; } = false;
	public bool HasTakenMap { get; set; } = false;
	public bool HasShownCrashSiteIntro { get; set; } = false;

	[Signal]
	public delegate void StationPowerChangedEventHandler(
		bool hasPower
	);

	public bool StationHasPower { get; private set; }

	public override void _Ready()
	{
		Instance = this;
	}

	public void RestoreStationPower() => SetStationPower(true);
	

	public void SetStationPower(bool hasPower)
	{
		if (StationHasPower == hasPower)
		{
			return;
		}

		StationHasPower = hasPower;

		EmitSignal(SignalName.StationPowerChanged, hasPower);
	}
}
