using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour
{
    [SerializeField]
    private Image _icon;

    [SerializeField]
    private TextMeshProUGUI _label;

    [SerializeField]
    private GameObject _stackContainer;

    [SerializeField]
    private TextMeshProUGUI _stackCount;

    public void Set(InventoryItem item)
    {
        _icon.sprite = item.Data.Icon;
        _label.text = item.Data.DisplayName;
        if(item.StackSize <= 1)
        {
            _stackContainer.SetActive(false);
            return;
        }

        _stackCount.text = item.StackSize.ToString();
    }
}
