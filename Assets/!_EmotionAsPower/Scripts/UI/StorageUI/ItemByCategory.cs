using System.Collections.Generic;
using UnityEngine;
using LgTyUtils;
using UnityEngine.UI;

public class ItemByCategory : MonoBehaviour
{
    [SerializeField]
    private ItemCategory category;
    [SerializeField]
    private Transform content;
    [SerializeField]
    private GameObject itemInStoragePrefab;
    [SerializeField]
    private TMPro.TextMeshProUGUI categoryTitle;

    public ItemCategory Category => category;

    public void Initialize(ItemCategory itemCategory, SerializableDictionary<string, int> allItems)
    {
        category = itemCategory;

        Button button = GetComponentInChildren<Button>();
        Image buttonImage = button?.GetComponent<Image>();  // Add null check for safety

        if (buttonImage != null)
        {
            var iconsDict = ContentManager.Instance?.categoryIcons?.itemCategoryIcons;
            if (iconsDict == null)
            {
                Debug.LogError($"Category icons dictionary is null for {category}");
            }
            else if (iconsDict.ContainsKey(category))
            {
                buttonImage.sprite = iconsDict[category];
            }
            else
            {
                Debug.LogWarning($"Category icon not found for {category}. Using default or skipping.");
                // Optionally set a default sprite here: buttonImage.sprite = someDefaultSprite;
            }
        }

        if (categoryTitle != null)
        {
            categoryTitle.text = itemCategory.ToString();
        }

        ClearItems();

        bool hasItems = false;
        var itemSODict = ContentManager.Instance?.ItemSOs;
        if (itemSODict == null)
        {
            Debug.LogError("ItemSOs dictionary is null.");
            gameObject.SetActive(false);
            return;
        }

        foreach (var itemPair in allItems.Pairs)
        {
            if (itemSODict.ContainsKey(itemPair.key))
            {
                ItemSO itemSO = itemSODict[itemPair.key];
                if (itemSO.category == category)
                {
                    GameObject itemObj = Instantiate(itemInStoragePrefab, content);
                    ItemInStorage itemInStorage = itemObj.GetComponent<ItemInStorage>();
                    if (itemInStorage != null)
                    {
                        itemInStorage.SetData(itemSO, itemPair.value);
                    }
                    hasItems = true;
                }
            }
            else
            {
                Debug.LogWarning($"ItemSO not found for key: {itemPair.key}. Skipping.");
            }
        }

        gameObject.SetActive(hasItems);
    }

    private void ClearItems()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    public void ToggleList()
    {
        if (content.gameObject.activeSelf == true) { 
            content.gameObject.SetActive(false);
        }
        else
        {
            content.gameObject.SetActive(true);
        }
    }
}