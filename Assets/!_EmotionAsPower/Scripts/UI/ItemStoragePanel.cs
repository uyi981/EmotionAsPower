using LgTyUtils;
using UnityEngine;

public class ItemStoragePanel : MonoBehaviour
{
    [SerializeField]
    private Transform content;
    [SerializeField]
    private GameObject itemInStoragePrefab;

    private void Start()
    {
        ItemStorage.Instance.OnStoragedItemsChange += UpdateUI;
    }

    public void UpdateUI(SerializableDictionary<string, int> items)
    {
        // Clear existing UI items
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Add new items
        foreach (var item in items)
        {
            Debug.Log(item.Key);
            ItemSO itemSO = ContentManager.Instance.ItemSOs[item.Key];

            GameObject itemObj = Instantiate(itemInStoragePrefab, content);

            ItemInStorage itemInStorage = itemObj.GetComponent<ItemInStorage>();
            if (itemInStorage != null)
            {
                itemInStorage.SetData(itemSO, item.Value);
            }
        }
    }
}