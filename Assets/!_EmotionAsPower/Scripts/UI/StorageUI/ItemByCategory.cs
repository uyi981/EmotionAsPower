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
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null) {
            //if (ContentManager.Instance.categoryIcons.itemCategoryIcons == null)
            //{
            //    Debug.Log("Missing config");
            //}
            if (!ContentManager.Instance.categoryIcons.itemCategoryIcons.ContainsKey(category))
            {
                Debug.Log("Category not found");
            }
            buttonImage.sprite = ContentManager.Instance.categoryIcons.itemCategoryIcons[category];

        }


        if (categoryTitle != null)
        {
            categoryTitle.text = itemCategory.ToString();
        }

        ClearItems();

        bool hasItems = false;
        foreach (var itemPair in allItems.Pairs)
        {
            
            Debug.Log(itemPair.key.ToString());
            ItemSO itemSO = ContentManager.Instance.ItemSOs[itemPair.key];
            
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