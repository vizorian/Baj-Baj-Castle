using UnityEngine;

[CreateAssetMenu(menuName = "Inventory Item Data")]
public class InventoryItemData : ScriptableObject
{
    public string Id;
    public string DisplayName;
    public int MaxStackSize = 1;
    public Sprite Icon;
    public GameObject Prefab;
    public ItemType ItemType;
    public ItemProperties ItemProperties;
}
