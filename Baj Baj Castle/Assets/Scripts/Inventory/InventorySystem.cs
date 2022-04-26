using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventorySystem : MonoBehaviour
{
    // Singleton for a single system
    private static InventorySystem instance;
    public static InventorySystem Instance { get { return instance; } }
    public UnityEvent OnInventoryChanged;
    public List<InventoryItem> Inventory { get; private set; }
    public int Capacity = 9;
    public InventoryItem SelectedItem { get; private set; }
    private int selectedItemIndex;
    private ActorHand hand;

    private void Awake()
    {
        Inventory = new List<InventoryItem>();
        SelectedItem = null;
        selectedItemIndex = -1;

        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;

        if (OnInventoryChanged == null)
            OnInventoryChanged = new UnityEvent();
    }

    private void Update()
    {
        if (hand == null)
            hand = GetComponentInChildren<ActorHand>();
    }

    private void PrintInventory()
    {
        string temp = "";
        for (int i = 0; i < Inventory.Count; i++)
        {
            temp += i + $":{Inventory[i].Data.DisplayName}" + " --- ";
        }
        Debug.Log(temp);
    }

    // Adds an item to inventory and invokes OnInventoryChanged
    public bool Add(InventoryItemData itemData)
    {
        bool success = true;
        InventoryItem newItem = new InventoryItem(itemData);
        // probably bad finding
        var existingItem = Inventory.Find(item => item.Data.Id == itemData.Id && item.StackSize < item.Data.MaxStackSize); // item of same type but not full
        if (existingItem == null) // non-full stackable item doesn't exist
        {
            if (Inventory.Count < Capacity)
            {
                Inventory.Add(newItem);
            }
            else
            {
                success = false;
            }
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
                if (selectedItemIndex != 0)
                {
                    selectedItemIndex--;
                }
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
        if (SelectedItem != null)
        {
            // Drop the actual item in the world
            var itemObject = Instantiate(SelectedItem.Data.Prefab, hand.transform.position, Quaternion.identity);
            LevelManager.Instance.AddItem(itemObject);

            // Remove the item from inventory
            Remove(SelectedItem.Data);
        }
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
