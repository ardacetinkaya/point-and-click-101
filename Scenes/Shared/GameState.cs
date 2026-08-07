using Godot;

public partial class GameState : Node
{
	public static GameState Instance { get; private set; } = null!;
	private Vector2? _loadedPlayerPosition;
	
	public string NextSpawnPoint { get; set; } = string.Empty;
	public bool HasLookedAtMap { get; set; } = false;
	public bool HasTakenMap { get; set; } = false;
	public bool HasShownCrashSiteIntro { get; set; } = false;
	public bool HasReceivedParkingTicket { get; set; } = false;
	public bool HasTakenParkingTicket { get; set; } = false;

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

	public void Reset()
	{
		NextSpawnPoint = string.Empty;
		_loadedPlayerPosition = null;
		HasLookedAtMap = false;
		HasTakenMap = false;
		HasShownCrashSiteIntro = false;
		HasReceivedParkingTicket = false;
		HasTakenParkingTicket = false;
		SetStationPower(false);
	}

	public void SetLoadedPlayerPosition(Vector2? position) => _loadedPlayerPosition = position;

	public bool TryTakeLoadedPlayerPosition(out Vector2 position)
	{
		if (!_loadedPlayerPosition.HasValue)
		{
			position = default;
			return false;
		}

		position = _loadedPlayerPosition.Value;
		_loadedPlayerPosition = null;
		return true;
	}

	public void Restore(
		bool hasLookedAtMap,
		bool hasTakenMap,
		bool hasShownCrashSiteIntro,
		bool hasReceivedParkingTicket,
		bool hasTakenParkingTicket,
		bool stationHasPower
	)
	{
		NextSpawnPoint = string.Empty;
		HasLookedAtMap = hasLookedAtMap;
		HasTakenMap = hasTakenMap;
		HasShownCrashSiteIntro = hasShownCrashSiteIntro;
		HasReceivedParkingTicket = hasReceivedParkingTicket;
		HasTakenParkingTicket = hasTakenParkingTicket;
		SetStationPower(stationHasPower);
	}
	

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
