using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInStorage : MonoBehaviour
{
    private Image image;
    private TextMeshProUGUI text;
    private void OnEnable()
    {
        image =  GetComponentInChildren<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetData(ItemSO itemSO, int amount)
    {
        image.sprite = itemSO.Icon;
        text.text = amount.ToString();
    }
}
