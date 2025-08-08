using LgTyUtils;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemLibrarySO", menuName = "Scriptable Objects/Item/ItemLibrarySO")]
public class ItemLibrarySO : ScriptableObject
{
    public ItemSO[] emotions;
    public ItemSO[] constructionMaterials;
    public ItemSO[] foods;
    public ItemSO[] specials;
    public SerializableDictionary<string, ItemSO> itemSOs;

    private void OnValidate()
    {
        // Initialize dictionary if null
        if (itemSOs == null)
        {
            itemSOs = new SerializableDictionary<string, ItemSO>();
        }
        else
        {
            itemSOs.Clear();
        }

        

        // Add items from each array in order
        AddItemsToDictionary(emotions);
        AddItemsToDictionary(constructionMaterials);
        AddItemsToDictionary(foods);
        AddItemsToDictionary(specials);
    }

    public void AddItemsToDictionary(ItemSO[] itemArray)
    {
        if (itemArray != null)
        {
            foreach (var item in itemArray)
            {
                if (item != null && !string.IsNullOrEmpty(item.ID))
                {
                    itemSOs[item.ID] = item;
                }
            }
        }
    }
}