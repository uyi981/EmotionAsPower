using System;
using LgTyUtils;
using Unity.VisualScripting;
using UnityEngine;

// TODO: Implementing object pool
public class ItemManager : Singleton<ItemManager>, IDataPersistence
{
    [SerializeField]
    private GameObject itemPrefab;
    public void LoadGame(GameData gameData)
    {
        ClearCurrentItems();
        InitializeLoadedItems(gameData.items);
    }

    public void SaveGame(ref GameData gameData)
    {
        gameData.items = FindAllItems();
    }

    public GameObject SpawnItem(ItemSO itemSO, int amount, Vector3 position)
    {
        //Spawn item object
        GameObject spawnedItem = Instantiate(itemPrefab, position, itemPrefab.transform.rotation, this.transform);

        Item item = spawnedItem.GetComponent<Item>();
        item.Initialize(itemSO, amount);
        return spawnedItem;
    }

    private void InitializeLoadedItems(ItemRuntimeInstance[] itemRuntimeInstances)
    {
        SerializableDictionary<string, ItemSO> itemSOs = ContentManager.Instance.ItemSOs;
        foreach (var itemInstance in itemRuntimeInstances)
        {
            itemSOs.TryGetValue(itemInstance.id, out var itemSO);
            if (itemSO != null)
            {
                SpawnItem(itemSO, itemInstance.amount, itemInstance.position + Vector3.up);
            }
            else
            {
                Debug.LogWarning($"ItemSO with ID {itemInstance.id} not found.");
            }
        }
    }   

    private ItemRuntimeInstance[] FindAllItems()
    {
        var items = GetComponentsInChildren<Item>();
        ItemRuntimeInstance[] result = new ItemRuntimeInstance[items.Length];
        for (int i = 0; i < items.Length; i++) {
            result[i] = new ItemRuntimeInstance(items[i].ItemSO.ID, items[i].Amount, items[i].transform.position);
        }
        return result;
    }

    private void ClearCurrentItems()
    {
        foreach(Item item in GetComponentsInChildren<Item>())
        {
            item.Clear();
        }
    }
}
