using LgTyUtils;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWave", menuName = "Scriptable Objects/EnemyWave")]
public class EnemyWave : ScriptableObject
{
    public SerializableDictionary<EnemySO, int> enemies;
}