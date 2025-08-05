using System.Collections.Generic;
using LgTyUtils;
using UnityEngine;

public class ItemStoragePanel : MonoBehaviour
{
    [SerializeField]
    private Transform content;
    [SerializeField]
    private GameObject itemByCategoryPrefab;

    private List<ItemByCategory> categoryPanels = new List<ItemByCategory>();

    private void Start()
    {
        GameManager.Instance.OnSetupFinished += () => InitializeCategoryPanels();
        ItemStorage.Instance.OnStoragedItemsChange += UpdateUI;
    }

    private void InitializeCategoryPanels()
    {
        ItemCategory[] categories = System.Enum.GetValues(typeof(ItemCategory)) as ItemCategory[];

        foreach (ItemCategory category in categories)
        {
            if (category == ItemCategory.Emotion) { 
                continue;
            }
            GameObject categoryObj = Instantiate(itemByCategoryPrefab, content);
            ItemByCategory itemByCategory = categoryObj.GetComponent<ItemByCategory>();

            if (itemByCategory != null)
            {
                categoryPanels.Add(itemByCategory);
                categoryObj.SetActive(false);
            }
        }
    }

    public void UpdateUI(SerializableDictionary<string, int> items)
    {
        if (!UIManager.Instance.ShowUI) return;

        if (items == null)
        {
            foreach (var categoryPanel in categoryPanels)
            {
                categoryPanel.gameObject.SetActive(false);
            }
            return;
        }

        ItemCategory[] categories = System.Enum.GetValues(typeof(ItemCategory)) as ItemCategory[];

        for (int i = 0; i < categories.Length && i < categoryPanels.Count; i++)
        {
            categoryPanels[i].Initialize(categories[i], items);
        }
    }

    private void OnDestroy()
    {
        if (ItemStorage.Instance != null)
        {
            ItemStorage.Instance.OnStoragedItemsChange -= UpdateUI;
        }
    }
}