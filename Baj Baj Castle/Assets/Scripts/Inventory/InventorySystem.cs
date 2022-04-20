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
    private Dictionary<InventoryItemData, InventoryItem> _itemDictionary;
    public List<InventoryItem> Inventory { get; private set; }
    public InventoryItem SelectedItem { get; private set; }
    private int selectedItemIndex;
    private ActorHand hand;

    private void Awake()
    {
        Inventory = new List<InventoryItem>();
        _itemDictionary = new Dictionary<InventoryItemData, InventoryItem>();
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
        //PrintInventory();
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
    public void Add(InventoryItemData itemData)
    {
        if (_itemDictionary.TryGetValue(itemData, out InventoryItem value))
        {
            value.AddToStack();
        }
        else
        {
            InventoryItem newItem = new InventoryItem(itemData);
            Inventory.Add(newItem);
            _itemDictionary.Add(itemData, newItem);

            // First pickup is selected automatically
            if (SelectedItem == null)
            {

                SelectedItem = newItem;
                selectedItemIndex = 0;

                hand.SetHeldItem(SelectedItem);
            }
        }
        OnInventoryChanged.Invoke();
    }

    // Removes an item from inventory and invokes OnInventoryChanged
    public void Remove(InventoryItemData itemData)
    {
        if (_itemDictionary.TryGetValue(itemData, out InventoryItem value))
        {
            value.RemoveFromStack();

            if (value.StackSize == 0)
            {
                Inventory.Remove(value);
                _itemDictionary.Remove(itemData);

                // If removed item was selected
                if (value == SelectedItem && Inventory.Count != 0) // If more items remain
                {
                    SelectedItem = Inventory[0];
                    selectedItemIndex = 0;

                    hand.SetHeldItem(SelectedItem);
                }
                else // If no items remain
                {
                    SelectedItem = null;
                    selectedItemIndex = -1;

                    hand.ClearHeldItem();
                }
            }
        }
        OnInventoryChanged.Invoke();
    }

    // Drops an item and calls Remove
    public void Drop()
    {
        if (SelectedItem != null)
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Drop the actual item in the world
            Instantiate(SelectedItem.Data.Prefab, new Vector3(mousePos.x, mousePos.y), Quaternion.identity); // FIX THIS to drop towards mouse

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
