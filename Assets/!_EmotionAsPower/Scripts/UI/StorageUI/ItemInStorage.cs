using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemInStorage : MonoBehaviour
{
    private Image image;
    private TextMeshProUGUI text;
    private ItemSO itemSO;

    public void SetData(ItemSO itemSO, int amount)
    {
        image = GetComponentInChildren<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        image.sprite = itemSO.Icon;
        text.text = amount.ToString();
        this.itemSO = itemSO;
    }

    public void ShowItemInfo()
    {
        Vector2 mousePosition = InputManager.Instance.mousePos.ReadValue<Vector2>();

        UIManager.Instance.ShowItemInfoPanel(this.itemSO, mousePosition);
    }
}
