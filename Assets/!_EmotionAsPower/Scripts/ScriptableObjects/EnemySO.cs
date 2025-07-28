using LgTyUtils;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : BaseScriptableObject
{
    public EnemyDefaultData defaultData;
    public AIBehaviour behaviour;
    public DropableItem[] dropableItems;
}
