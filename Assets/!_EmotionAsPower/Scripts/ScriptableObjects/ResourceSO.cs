using LgTyUtils;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceSO", menuName = "Scriptable Objects/ResourceSO")]
public class ResourceSO : BaseScriptableObject
{
    public int maxHealth;
    public float harvestDamage;
    public DropableItem[] dropableItems;
}
