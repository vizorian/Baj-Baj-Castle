using UnityEngine;

[CreateAssetMenu(menuName = "Inventory Item Data")]
public class InventoryItemData : ScriptableObject
{
    public string Id;
    public string DisplayName;
    public Sprite Icon;
    public GameObject Prefab;
    public ItemType itemType;
    public ItemProperties ItemProperties;
}
