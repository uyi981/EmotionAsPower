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
    public bool spawnEnemies; 
    public bool addRandomItems; 
    public int minItemAmount = 1; 
    public int maxItemAmount = 10; 

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
        if (addRandomItems)
        {
            AddRandomItemsToStorage();
            addRandomItems = false;
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
        }
    }

    [ContextMenu("Add Random Items to Storage")]
    public void AddRandomItemsToStorage()
    {
        SerializableDictionary<string, ItemSO> itemSOs = ContentManager.Instance.ItemSOs;
        if (itemSOs == null || itemSOs.Count == 0)
        {
            Debug.LogWarning("No ItemSOs found in ContentManager!");
            return;
        }

        List<ItemSO> itemList = itemSOs.Values.ToList();

        for (int i = 0; i < spawnAmount; i++)
        {
            ItemSO randomItemSO = itemList[Random.Range(0, itemList.Count)];
            int randomAmount = Random.Range(minItemAmount, maxItemAmount + 1);

            ItemStorage.Instance.AddItem(randomItemSO, randomAmount);
        }
    }
}