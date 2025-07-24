using System.Collections.Generic;
using System.Linq;
using LgTyUtils;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>, IDataPersistence
{
    [SerializeField]
    private GameObject enemyPrefab;

    public GameObject SpawnEnemy(EnemySO enemySO, Vector3 position)
    {
        if (enemyPrefab == null || enemySO == null)
        {
            Debug.LogError("Enemy prefab or EnemySO is null in EnemyManager!");
            return null;
        }

        GameObject spawnedEnemy = Instantiate(enemyPrefab, position, Quaternion.identity, this.transform);
        Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
        enemy.Initialize(enemySO);
        return spawnedEnemy;
    }

    public void LoadGame(GameData gameData)
    {
        ClearCurrentEnemies();
        InitializeLoadedEnemies(gameData.enemies);
    }

    public void SaveGame(ref GameData gameData)
    {
        gameData.enemies = FindAllEnemies();
    }

    private void ClearCurrentEnemies()
    {
        foreach (Enemy enemy in GetComponentsInChildren<Enemy>())
        {
            Destroy(enemy.gameObject);
        }
    }

    private void InitializeLoadedEnemies(EnemyRuntimeInstance[] enemyInstances)
    {
        SerializableDictionary<string, EnemySO> enemySOs = ContentManager.Instance.EnemySOs;
        foreach (var instance in enemyInstances)
        {
            if (enemySOs.TryGetValue(instance.id, out EnemySO enemySO))
            {
                GameObject spawnedEnemy = SpawnEnemy(enemySO, instance.position);
                Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
                enemy.Health.SetHealth(instance.currentHealth);
            }
            else
            {
                Debug.LogWarning($"EnemySO with ID {instance.id} not found.");
            }
        }
    }

    private EnemyRuntimeInstance[] FindAllEnemies()
    {
        Enemy[] enemies = GetComponentsInChildren<Enemy>();
        EnemyRuntimeInstance[] result = new EnemyRuntimeInstance[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];
            result[i] = new EnemyRuntimeInstance
            {
                id = enemy.EnemySO.ID,
                position = enemy.transform.position,
                currentHealth = enemy.Health.CurrentHealth
            };
        }
        return result;
    }
}