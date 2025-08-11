using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI descriptionLabel;
    [SerializeField] private TextMeshProUGUI categoryLabel;

    private ItemSO itemSO;

    public void Show(ItemSO itemSO)
    {
        if (itemSO == null) return;

        this.gameObject.SetActive(true);
        this.itemSO = itemSO;
        UpdateDisplay();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        this.itemSO = null;
    }

    private void UpdateDisplay()
    {
        ItemCategory category = itemSO.category;
        string displayCategory = category.ToDisplayString();
        if (itemSO == null) return;

        // Update item name
        if (nameLabel != null)
            nameLabel.text = itemSO.DisplayName;

        // Update item description
        if (descriptionLabel != null)
            descriptionLabel.text = itemSO.Description;

        // Update item category
        if (categoryLabel != null)
            categoryLabel.text = $"{displayCategory}";
    }

}