namespace Inventory;

[Serializable]
public class InventoryItem
{
    public InventoryItem(InventoryItemData data)
    {
        Data = data;
        AddToStack();
    }

    public InventoryItemData Data { get; private set; }
    public int StackSize { get; private set; }

    // Add to stack
    public void AddToStack()
    {
        StackSize++;
    }

    // Remove from stack
    public void RemoveFromStack()
    {
        StackSize--;
    }
}