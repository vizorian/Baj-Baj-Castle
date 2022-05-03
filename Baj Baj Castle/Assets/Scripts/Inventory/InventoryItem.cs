using System;

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

    public void AddToStack()
    {
        StackSize++;
    }

    public void RemoveFromStack()
    {
        StackSize--;
    }
}