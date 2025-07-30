using LgTyUtils;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawningConfig", menuName = "Scriptable Objects/EnemySpawningConfig")]
public class EnemySpawningConfig  : ScriptableObject
{
    public SerializableDictionary<StageOfDayCondition, EnemyWave> waves;
}