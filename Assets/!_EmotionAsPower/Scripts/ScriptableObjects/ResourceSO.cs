using LgTyUtils;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceSO", menuName = "Scriptable Objects/ResourceSO")]
public class ResourceSO : BaseScriptableObject
{
    public int maxHealth;
    public float harvestDamage;
    public DropableItem[] dropableItems;
}
