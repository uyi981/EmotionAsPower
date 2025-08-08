using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInStorage : MonoBehaviour
{
    private Image image;
    private TextMeshProUGUI text;

    public void SetData(ItemSO itemSO, int amount)
    {
        image = GetComponentInChildren<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        image.sprite = itemSO.Icon;
        text.text = amount.ToString();
    }
}
