using System;
using LgTyUtils;
using NUnit.Framework;
using UnityEngine;

public class ItemStorage : Singleton<ItemStorage>, IDataPersistence
{
    [SerializeField]
    private SerializableDictionary<string, int> storagedItems;
    public SerializableDictionary<string, int> StoragedItems => storagedItems;

    public Action<SerializableDictionary<string, int>> OnStoragedItemsChange;
    public void AddItem(Item item)
    {
        this.AddItem(item.ItemSO.ID, item.Amount);
    }

    public void AddItem(ItemSO itemSO, int amount)
    {
        this.AddItem(itemSO.ID, amount);
    }

    public void AddItem(string id, int amount)
    {
        if (amount <= 0) return; 

        if (storagedItems.ContainsKey(id))
        {
            storagedItems[id] += amount;
        }
        else
        {
            storagedItems.Add(id, amount);
        }
        OnStoragedItemsChange?.Invoke(storagedItems);
    }

    public int TryTakeItem(string id, int amount)
    {
        if (amount <= 0) return 0;

        if (storagedItems.ContainsKey(id) && storagedItems[id] >= amount)
        {
            storagedItems[id] -= amount;
            if (storagedItems[id] == 0)
            {
                storagedItems.Remove(id); 
            }
            return amount;
        }
        OnStoragedItemsChange?.Invoke(storagedItems);
        return 0;
    }

    public int TryTakeItem(ItemSO itemSO, int amount)
    {
        return TryTakeItem(itemSO.ID, amount);
    }

    public void LoadGame(GameData gameData)
    {
        this.storagedItems = gameData.storagedItems;
        OnStoragedItemsChange?.Invoke(storagedItems);
    }

    public void SaveGame(ref GameData gameData)
    {
        gameData.storagedItems = this.storagedItems;
    }
}