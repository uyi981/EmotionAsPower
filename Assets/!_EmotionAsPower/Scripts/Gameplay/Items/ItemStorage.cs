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

    private void Start()
    {
        GameManager.Instance.OnSetupFinished += () => OnStoragedItemsChange?.Invoke(StoragedItems);
    }
    //public SerializableDictionary<string, int> GetFoodItemsList()
    //{
    //    foreach (var item in storagedItems.Dictionary)
    //    {
    //        ItemManager.
    //    }
    //}
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
    public bool CheckListRequireItem(SerializableDictionary<ItemSO, int> itemList)
    {
        foreach (var item in itemList.Dictionary)
        {
            if (!storagedItems.ContainsKey(item.Key.ID) || storagedItems[item.Key.ID] < item.Value)
            {
                return false; // Not enough items
            }
        }
        return true;
    }
    public SerializableDictionary<ItemSO, int> CheckListRequireItemAndReturnItemMissing(SerializableDictionary<ItemSO, int> itemList)
    {
        SerializableDictionary<ItemSO, int> missingItems = new SerializableDictionary<ItemSO, int>();
        foreach (var item in itemList.Dictionary)
        {
            if (!storagedItems.ContainsKey(item.Key.ID) || storagedItems[item.Key.ID] < item.Value)
            {
                missingItems.Add(item.Key, item.Value - GetStoragedItemAmount(item.Key));
            }
        }
        return missingItems;
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
            OnStoragedItemsChange?.Invoke(storagedItems);
            return amount;
        }
        OnStoragedItemsChange?.Invoke(storagedItems);
        return 0;
    }

    public void SubtractItem(ItemSO itemSO, int amount)
    {
        string id = itemSO.ID;
        if (amount <= 0) return;

        if (storagedItems.ContainsKey(id) && storagedItems[id] >= amount)
        {
            storagedItems[id] -= amount;
            if (storagedItems[id] == 0)
            {
                storagedItems.Remove(id);
            }
            OnStoragedItemsChange?.Invoke(storagedItems);
            return;
        }
        if(storagedItems.ContainsKey(id) && storagedItems[id] < amount)
        {
            storagedItems.Remove(id);
        }
        OnStoragedItemsChange?.Invoke(storagedItems);
        return ;
    }

    public int TryTakeItem(ItemSO itemSO, int amount)
    {
        return TryTakeItem(itemSO.ID, amount);
    }

    public int Check(Item item, int amount)
    {
        return Check(item.ItemSO, amount);
    }

    public int Check(ItemSO itemSO, int amount)
    {
        return Check(itemSO.ID, amount);
    }

    public int Check(string id, int amount)
    {
        if (!storagedItems.ContainsKey(id)) return -amount;
        return storagedItems[id] - amount;
    }

    public int GetStoragedItemAmount(Item item)
    {
        return GetStoragedItemAmount(item.ItemSO);
    }

    public int GetStoragedItemAmount(ItemSO itemSO)
    {
        return GetStoragedItemAmount(itemSO.ID);
    }

    public int GetStoragedItemAmount(string id)
    {
        if(!storagedItems.ContainsKey(id)) return 0;
        return storagedItems[id];
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

    public void CallUpdateEvent()
    {
        OnStoragedItemsChange?.Invoke(storagedItems);
    }
}