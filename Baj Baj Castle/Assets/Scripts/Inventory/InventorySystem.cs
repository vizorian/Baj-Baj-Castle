using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class InventorySystem : MonoBehaviour
{
    // Singleton for a single system
    public int Capacity = 9;
    private ActorHand hand;
    public UnityEvent OnInventoryChanged;
    private int selectedItemIndex;
    public static InventorySystem Instance { get; private set; }

    public List<InventoryItem> Inventory { get; private set; }
    public InventoryItem SelectedItem { get; private set; }

    [UsedImplicitly]
    private void Awake()
    {
        Inventory = new List<InventoryItem>();
        SelectedItem = null;
        selectedItemIndex = -1;

        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        OnInventoryChanged ??= new UnityEvent();
    }

    [UsedImplicitly]
    private void Update()
    {
        if (hand == null)
            hand = GetComponentInChildren<ActorHand>();
    }

    // Adds an item to inventory and invokes OnInventoryChanged
    public bool Add(InventoryItemData itemData)
    {
        var success = true;
        var newItem = new InventoryItem(itemData);
        // probably bad finding
        var existingItem =
            Inventory.Find(item =>
                item.Data.Id == itemData.Id &&
                item.StackSize < item.Data.MaxStackSize); // item of same type but not full
        if (existingItem == null) // non-full stackable item doesn't exist
        {
            if (Inventory.Count < Capacity)
                Inventory.Add(newItem);
            else
                success = false;
        }
        else // non-full stackable item exists
        {
            existingItem.AddToStack();
        }

        // First pickup is selected automatically
        if (SelectedItem == null)
        {
            SelectedItem = newItem;
            selectedItemIndex = 0;

            hand.SetHeldItem(SelectedItem);
        }

        OnInventoryChanged.Invoke();
        return success;
    }

    // Removes an item from inventory and invokes OnInventoryChanged
    public void Remove(InventoryItemData itemData)
    {
        SelectedItem.RemoveFromStack();
        if (SelectedItem.StackSize == 0) // if last item dropped
        {
            Inventory.RemoveAt(selectedItemIndex);

            if (Inventory.Count != 0) // If more items remain
            {
                if (selectedItemIndex != 0) selectedItemIndex--;
                SelectedItem = Inventory[selectedItemIndex];
                hand.SetHeldItem(SelectedItem);
            }
            else // If no items remain
            {
                SelectedItem = null;
                selectedItemIndex = -1;

                hand.ClearHeldItem();
            }
        }

        OnInventoryChanged.Invoke();
    }

    // Drops an item and calls Remove
    public void Drop()
    {
        if (SelectedItem == null) return;

        // Drop the actual item in the world
        var itemObject = Instantiate(SelectedItem.Data.Prefab, hand.transform.position, Quaternion.identity);
        LevelManager.Instance.AddItem(itemObject);

        // Remove the item from inventory
        Remove(SelectedItem.Data);
    }

    public void Next()
    {
        // If inventory has a single item
        // or next item is out of bounds return
        if (Inventory.Count == 1 || selectedItemIndex + 1 == Inventory.Count)
            return;

        if (selectedItemIndex + 1 < Inventory.Count)
            selectedItemIndex++;

        SelectedItem = Inventory[selectedItemIndex];

        hand.SetHeldItem(SelectedItem);
        OnInventoryChanged.Invoke();
    }

    public void Previous()
    {
        // If inventory has a single item
        // or previous is nonexistant return
        if (Inventory.Count == 1 || selectedItemIndex <= 0)
            return;

        if (selectedItemIndex - 1 >= 0)
            selectedItemIndex--;

        SelectedItem = Inventory[selectedItemIndex];

        hand.SetHeldItem(SelectedItem);
        OnInventoryChanged.Invoke();
    }
}