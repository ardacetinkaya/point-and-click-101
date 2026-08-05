using Godot;
// using PointAndClick101.Scenes.Shared;

public partial class InventoryPanel : Control
{
    [ExportGroup("Item Icons")]

    [Export]
    public Texture2D MapIcon { get; set; } = null!;

    private GridContainer _itemGrid = null!;

    public override void _Ready()
    {
        _itemGrid = GetNode<GridContainer>(
            "CenterContainer/Window/MarginContainer/Content/ItemGrid"
        );

        
        InventoryManager.Instance.InventoryChanged +=
            RefreshItems;

        Hide();
        RefreshItems();
    }

    public override void _UnhandledInput(
        InputEvent inputEvent
    )
    {
        if (!inputEvent.IsActionPressed("toggle_inventory"))
        {
            return;
        }

        ToggleInventory();
        GetViewport().SetInputAsHandled();
    }

    private void ToggleInventory()
    {
        if (Visible)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    private void OpenInventory()
    {
        ActionMenu? actionMenu = GetNodeOrNull<ActionMenu>("../ActionMenu");
        actionMenu?.HideMenu();
        
        RefreshItems();
        Show();
    }

    private void CloseInventory()
    {
        Hide();
    }

    private void RefreshItems()
    {
        ClearGrid();

        foreach (string itemId in InventoryManager.Instance.Items)
        {
            Button button = CreateItemButton(itemId);
            _itemGrid.AddChild(button);
        }
    }

    private Button CreateItemButton(string itemId)
    {
        var button = new Button
        {
            Text = string.Empty,
            Icon = GetItemIcon(itemId),
            TooltipText = GetItemName(itemId),
            CustomMinimumSize = new Vector2(76, 76),
            Flat = true,
            FocusMode = FocusModeEnum.None,
            ExpandIcon = true,
            IconAlignment = HorizontalAlignment.Center,
            VerticalIconAlignment = VerticalAlignment.Center
        };
        
        button.AddThemeConstantOverride(
            "icon_max_width",
            64
        );
        
        return button;
    }

    private Texture2D GetItemIcon(string itemId)
    {
        return itemId switch
        {
            InventoryItemIds.Map => MapIcon,
            _ => null!
        };
    }

    private string GetItemName(string itemId)
    {
        return itemId switch
        {
            InventoryItemIds.Map => "Map",
            _ => itemId
        };
    }

    private void ClearGrid()
    {
        foreach (Node child in _itemGrid.GetChildren())
        {
            _itemGrid.RemoveChild(child);
            child.QueueFree();
        }
    }

    public override void _ExitTree()
    {
        InventoryManager.Instance.InventoryChanged -=
            RefreshItems;
    }
}