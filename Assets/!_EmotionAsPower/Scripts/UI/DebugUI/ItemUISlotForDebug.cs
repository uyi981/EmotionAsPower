using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUISlotForDebug : MonoBehaviour
{
    private Image image;
    private TextMeshProUGUI text;
    private ItemSO itemSO;
    private int amountToDebug;
    private void OnEnable()
    {
        image = GetComponentInChildren<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetData(ItemSO itemSO, int amount)
    {
        this.itemSO = itemSO;
        amountToDebug = amount;
        image.sprite = itemSO.Icon;
        text.text = amount.ToString();
    }

    public void Debug_AddItem()
    {
        ItemStorage.Instance.AddItem(itemSO, amountToDebug);
    }

    public void Debug_TakeItem()
    {
        ItemStorage.Instance.TryTakeItem(itemSO, amountToDebug);
    }
}
