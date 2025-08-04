using System.Collections.Generic;
using System.Linq;
using LgTyUtils;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>, IDataPersistence
{
    [SerializeField]
    private GameObject enemyPrefab;

    [SerializeField]
    private  EnemySpawningConfig enemySpawningConfig;

    [Header("Spawn Settings")]
    [SerializeField]
    private float spawnRadius = 10f;
    [SerializeField]
    private Vector3 spawnCenter = Vector3.zero;

    private void Start()
    {
        DayTimeController.Instance.OnStageOfDayChanged += SpawnEnemyWave;
    }

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

    public GameObject SpawnEnemyOnCircle(EnemySO enemySO)
    {
        Vector3 spawnPosition = GetRandomCirclePosition();
        return SpawnEnemy(enemySO, spawnPosition);
    }

    public GameObject SpawnEnemyOnCircle(EnemySO enemySO, float customRadius)
    {
        Vector3 spawnPosition = GetRandomCirclePosition(customRadius);
        return SpawnEnemy(enemySO, spawnPosition);
    }

    public GameObject[] SpawnMultipleEnemiesOnCircle(EnemySO enemySO, int count)
    {
        GameObject[] spawnedEnemies = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            spawnedEnemies[i] = SpawnEnemyOnCircle(enemySO);
        }

        return spawnedEnemies;
    }

    public GameObject[] SpawnEnemiesInFormation(EnemySO enemySO, int count, float radius = -1f)
    {
        if (radius < 0) radius = spawnRadius;

        GameObject[] spawnedEnemies = new GameObject[count];
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 spawnPosition = spawnCenter + new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            spawnedEnemies[i] = SpawnEnemy(enemySO, spawnPosition);
        }

        return spawnedEnemies;
    }

    private Vector3 GetRandomCirclePosition(float radius = -1f)
    {
        if (radius < 0) radius = spawnRadius;

        // Generate random angle
        float angle = Random.Range(0f, 2f * Mathf.PI);

        // Calculate position on circle circumference
        Vector3 position = spawnCenter + new Vector3(
            Mathf.Cos(angle) * radius,
            0f,
            Mathf.Sin(angle) * radius
        );

        return position;
    }

    public void SetSpawnParameters(float radius, Vector3 center)
    {
        spawnRadius = radius;
        spawnCenter = center;
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
                enemy.UpdateExistingTime(instance.remainExistingTime);
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
                currentHealth = enemy.Health.CurrentHealth,
                remainExistingTime = enemy.ExistingTimer

            };
        }
        return result;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw spawn circle in scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnCenter, spawnRadius);

        // Draw center point
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(spawnCenter, 0.2f);
    }
#endif

    public void SpawnEnemyWave(StageOfDay stageOfDayCondition)
    {
        // If there is no wave on this stage of the day then return
        if (!enemySpawningConfig.waves.ContainsKey(stageOfDayCondition)) return;
        EnemyWave enemyWave = enemySpawningConfig.waves[stageOfDayCondition];

        var enemies = enemyWave.enemies;
        foreach (var enemy in enemies)
        {
            int spawnedAmount = 0;
            while (spawnedAmount < enemy.Value)
            {
                SpawnEnemyOnCircle(enemy.Key, spawnRadius);
                spawnedAmount++;
            }

        }
    }
}