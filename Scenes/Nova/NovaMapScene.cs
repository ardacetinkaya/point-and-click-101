using Godot;

public partial class NovaMapScene : Node2D
{
    private MapLocationHotspot _powerOffline = null!;
    private MapLocationHotspot _crashSite = null!;
    private MapLocationHotspot _krakerSite = null!;
    
    public override void _Ready()
    {
        _powerOffline = GetNode<MapLocationHotspot>("Hotspots/PowerOffline");
        _crashSite = GetNode<MapLocationHotspot>("Hotspots/CrashSite");
        _krakerSite = GetNode<MapLocationHotspot>("Hotspots/Kraker");

        _powerOffline.Selected += OnPowerOfflineSelected;
        _crashSite.Selected += OnCrashSiteSelected;
        _krakerSite.Selected += OnKrakerSiteSelected;
    }

    private void OnPowerOfflineSelected()
    {
        GameState.Instance.NextSpawnPoint = "FromNovaMap";

        Callable.From(ChangeToPowerOffline)
            .CallDeferred();
    }

    private void OnCrashSiteSelected()
    {
        GameState.Instance.NextSpawnPoint = "FromNovaMap";

        Callable.From(ChangeToCrashSite)
            .CallDeferred();
    }
    
    private void OnKrakerSiteSelected()
    {
        GameState.Instance.NextSpawnPoint = "FromNovaMap";

        Callable.From(ChangeToKrakerSite)
            .CallDeferred();
    }
    
    private void ChangeScene(string scenePath)
    {
        Error result = GetTree().ChangeSceneToFile(scenePath);

        if (result == Error.Ok)
        {
            return;
        }
        
        GameState.Instance.NextSpawnPoint = string.Empty;

        GD.PushError($"Could not change scene: {result}");
    }
    
    private void ChangeToPowerOffline() => ChangeScene(Consts.PowerOfflineScenePath);

    private void ChangeToCrashSite() => ChangeScene(Consts.CrashSiteScenePath);

    private void ChangeToKrakerSite() => ChangeScene(Consts.KrakerSiteScenePath);
}
