using UnityEngine;

[CreateAssetMenu(menuName = "Inventory Item Data")]
public class InventoryItemData : ScriptableObject
{
    public string DisplayName;
    public Sprite Icon;
    public string Id;
    public ItemProperties ItemProperties;
    public ItemType ItemType;
    public int MaxStackSize = 1;
    public GameObject Prefab;
}