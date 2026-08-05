using Godot;
using System.Collections.Generic;

public partial class InventoryManager : Node
{
    public static InventoryManager Instance { get; private set; } = null!;

    private readonly List<string> _items = [];

    public IReadOnlyList<string> Items => _items;

    [Signal]
    public delegate void InventoryChangedEventHandler();

    public override void _Ready()
    {
        Instance = this;
    }

    public bool AddItem(string itemId)
    {
        if (_items.Contains(itemId))
        {
            return false;
        }

        _items.Add(itemId);
        EmitSignal(SignalName.InventoryChanged);

        return true;
    }

    public bool RemoveItem(string itemId)
    {
        bool removed = _items.Remove(itemId);

        if (removed)
        {
            EmitSignal(SignalName.InventoryChanged);
        }

        return removed;
    }

    public bool HasItem(string itemId)
    {
        return _items.Contains(itemId);
    }
}