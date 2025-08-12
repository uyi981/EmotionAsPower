using System;
using LgTyUtils;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWave", menuName = "Scriptable Objects/EnemyWave")]
public class EnemyWave : ScriptableObject
{
    [Serializable]
    public struct EnemySpawnData
    {
        public int count; // Number of enemies to spawn
        public float spawnAngle; // Angle in degrees (0-360) for spawning direction
    }

    public SerializableDictionary<GameObject, EnemySpawnData[]> enemies;
}