using LgTyUtils;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemCategoryIconConfig", menuName = "Scriptable Objects/UI/ItemCategoryIconConfig")]
public class ItemCategoryIconConfig : ScriptableObject
{
    public SerializableDictionary<ItemCategory, Sprite> itemCategoryIcons;
}