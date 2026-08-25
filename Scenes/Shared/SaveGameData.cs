using System.Collections.Generic;

public sealed class SaveGameData
{
	public int Version { get; set; } = 1;
	public string SavedAt { get; set; } = string.Empty;
	public string ScenePath { get; set; } = string.Empty;
	public float? PlayerPositionX { get; set; }
	public float? PlayerPositionY { get; set; }
	public bool HasLookedAtMap { get; set; }
	public bool HasTakenMap { get; set; }
	public bool HasShownCrashSiteIntro { get; set; }
	public bool HasReceivedParkingTicket { get; set; }
	public bool HasTakenParkingTicket { get; set; }
	public bool StationHasPower { get; set; }
	public List<string> InventoryItems { get; set; } = [];
}
