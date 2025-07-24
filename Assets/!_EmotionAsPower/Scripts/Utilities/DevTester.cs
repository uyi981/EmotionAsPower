using System.Collections.Generic;
using System.Linq;
using LgTyUtils;
using UnityEngine;
using Random = UnityEngine.Random;

public class DevTester : Singleton<DevTester>
{
    public int spawnAmount;
    public float spawnRange;
    public bool spawn;
    public bool spawnEnemies; // New flag for spawning enemies

    private void Update()
    {
        if (spawn)
        {
            SpawnRandomResources();
            spawn = false;
        }
        if (spawnEnemies)
        {
            SpawnRandomEnemies();
            spawnEnemies = false;
        }
    }

    [ContextMenu("Spawn Random Resources")]
    public void SpawnRandomResources()
    {
        SerializableDictionary<string, ResourceSO> resourceSOs = ContentManager.Instance.ResourceSOs;
        if (resourceSOs == null || resourceSOs.Count == 0)
        {
            Debug.LogWarning("No ResourceSOs found in ContentManager!");
            return;
        }

        List<ResourceSO> resourceList = resourceSOs.Values.ToList();

        for (int i = 0; i < spawnAmount; i++)
        {
            ResourceSO randomResourceSO = resourceList[Random.Range(0, resourceList.Count)];
            Vector3 randomPosition = transform.position + new Vector3(
                Random.Range(-spawnRange, spawnRange),
                0f,
                Random.Range(-spawnRange, spawnRange)
            );
            ResourceManager.Instance.SpawnResource(randomResourceSO, randomPosition);
            Debug.Log($"Spawned resource '{randomResourceSO.DisplayName}' at position {randomPosition}");
        }
    }

    [ContextMenu("Spawn Random Enemies")]
    public void SpawnRandomEnemies()
    {
        SerializableDictionary<string, EnemySO> enemySOs = ContentManager.Instance.EnemySOs;
        if (enemySOs == null || enemySOs.Count == 0)
        {
            Debug.LogWarning("No EnemySOs found in ContentManager!");
            return;
        }

        List<EnemySO> enemyList = enemySOs.Values.ToList();

        for (int i = 0; i < spawnAmount; i++)
        {
            EnemySO randomEnemySO = enemyList[Random.Range(0, enemyList.Count)];
            Vector3 randomPosition = transform.position + new Vector3(
                Random.Range(-spawnRange, spawnRange),
                0f,
                Random.Range(-spawnRange, spawnRange)
            );
            EnemyManager.Instance.SpawnEnemy(randomEnemySO, randomPosition);
            Debug.Log($"Spawned enemy '{randomEnemySO.DisplayName}' at position {randomPosition}");
        }
    }
}