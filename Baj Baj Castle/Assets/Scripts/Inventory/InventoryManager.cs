using JetBrains.Annotations;
using UnityEngine;

[UsedImplicitly]
public class InventoryManager : MonoBehaviour
{
    public GameObject SlotPrefab;
    public GameObject SlotSelectedPrefab;

    [UsedImplicitly]
    private void Start()
    {
        InventorySystem.Instance.OnInventoryChanged.AddListener(UpdateInventory);
    }

    private void UpdateInventory()
    {
        foreach (Transform t in transform) Destroy(t.gameObject);

        DrawInventory();
    }

    private void DrawInventory()
    {
        foreach (var item in InventorySystem.Instance.Inventory)
            AddInventorySlot(item, item == InventorySystem.Instance.SelectedItem ? SlotSelectedPrefab : SlotPrefab);
    }

    private void AddInventorySlot(InventoryItem item, GameObject prefab)
    {
        var obj = Instantiate(prefab);
        obj.transform.SetParent(transform, false);

        var slot = obj.GetComponent<InventoryItemSlot>();
        slot.Set(item);
    }
}