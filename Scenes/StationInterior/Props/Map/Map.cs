using Godot;
using System.Collections.Generic;

public partial class Map : Hotspot
{
    [Export]
    public bool CanLook { get; set; } = true;

    [Export]
    public bool CanTouch { get; set; } = true;
    
    
    public override void _Ready()
    {
        base._Ready();
        
        if (GameState.Instance.HasTakenMap)
        {
            QueueFree();
        }
    }
    
    public override string ExecuteAction(HotspotAction action)
    {
        return action switch
        {
            HotspotAction.Look => LookAtMap(),
            HotspotAction.Touch => TakeMap(),
            _ => "There is nothing that I can do now"
        };
    }

    public override IEnumerable<HotspotAction> GetAvailableActions()
    {
        var actions = new List<HotspotAction>
        {
            HotspotAction.Look
        };

        if (GameState.Instance.HasLookedAtMap)
        {
            actions.Add(HotspotAction.Touch);
        }

        return actions;
    }
    
    private string TakeMap()
    {
        if (GameState.Instance.HasTakenMap)
        {
            return "I already took the map.";
        }

        bool added = InventoryManager.Instance.AddItem(InventoryItemIds.Map);

        if (!added)
        {
            return "I already have the map.";
        }

        GameState.Instance.HasTakenMap = true;

        QueueFree();

        return "Now I know where Waldo is.";
    }
    
    
    private string LookAtMap()
    {
        GameState.Instance.HasLookedAtMap = true;
        
        return "A map...I can find Waldo with it. It might be good to take this...or maybe not...";
    }
}
