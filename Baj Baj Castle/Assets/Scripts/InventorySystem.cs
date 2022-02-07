using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventorySystem : MonoBehaviour
{
    // Singleton for a single system
    private static InventorySystem _instance;
    public static InventorySystem Instance { get { return _instance; } }

    public UnityEvent OnInventoryChanged;

    private Dictionary<InventoryItemData, InventoryItem> _itemDictionary;

    public List<InventoryItem> Inventory { get; private set; }
    public InventoryItem selectedItem { get; private set; }
    private int selectedItemIndex;


    private void Awake()
    {
        Inventory = new List<InventoryItem>();
        _itemDictionary = new Dictionary<InventoryItemData, InventoryItem>();
        selectedItem = null;
        selectedItemIndex = -1;

        if(_instance != null && _instance != this)
            Destroy(gameObject);
        else
            _instance = this;

        if (OnInventoryChanged == null)
            OnInventoryChanged = new UnityEvent();
    }

    private void Update()
    {
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
        if(_itemDictionary.TryGetValue(itemData, out InventoryItem value))
        {
            value.AddToStack();
        }
        else
        {
            InventoryItem newItem = new InventoryItem(itemData);
            Inventory.Add(newItem);
            _itemDictionary.Add(itemData, newItem);

            // First pickup is selected automatically
            if (selectedItem == null)
            {
                selectedItem = newItem;
                selectedItemIndex = 0;
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

            if(value.StackSize == 0)
            {
                Inventory.Remove(value);
                _itemDictionary.Remove(itemData);

                // If removed item was selected
                if (value == selectedItem && Inventory.Count != 0) // If more items remain
                {
                    selectedItem = Inventory[0];
                    selectedItemIndex = 0;
                }
                else // If no items remain
                {
                    selectedItem = null;
                    selectedItemIndex = -1;
                }
            }
        }
        OnInventoryChanged.Invoke();
    }

    public void Next()
    {
        // If inventory has a single item
        // or next item is out of bounds return
        if (Inventory.Count == 1 || selectedItemIndex + 1 == Inventory.Count)
            return;

        if(selectedItemIndex + 1 < Inventory.Count)
            selectedItemIndex++;
        
        selectedItem = Inventory[selectedItemIndex];

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

        selectedItem = Inventory[selectedItemIndex];

        OnInventoryChanged.Invoke();
    }
}
