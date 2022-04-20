using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject SlotPrefab;
    public GameObject SlotSelectedPrefab;

    void Start()
    {
        InventorySystem.Instance.OnInventoryChanged.AddListener(UpdateInventory);
    }

    private void UpdateInventory()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }

        DrawInventory();
    }

    private void DrawInventory()
    {
        foreach (InventoryItem item in InventorySystem.Instance.Inventory)
        {
            if (item == InventorySystem.Instance.SelectedItem)
                AddInventorySlot(item, SlotSelectedPrefab);
            else
                AddInventorySlot(item, SlotPrefab);
        }
    }

    private void AddInventorySlot(InventoryItem item, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        obj.transform.SetParent(transform, false);

        InventoryItemSlot slot = obj.GetComponent<InventoryItemSlot>();
        slot.Set(item);
    }
}
