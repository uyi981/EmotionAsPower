using System.Collections.Generic;
using System.Linq;
using LgTyUtils;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>, IDataPersistence
{
    [SerializeField]
    private EnemySpawningConfig enemySpawningConfig;

    public EnemySpawningConfig SpawningConfig => enemySpawningConfig;

    [Header("Spawn Settings")]
    [SerializeField]
    private float spawnRadius = 10f;
    [SerializeField]
    private Vector3 spawnCenter = Vector3.zero;

    private void Start()
    {
        DayTimeController.Instance.OnStageOfDayChanged += SpawnEnemyWave;
    }

    public GameObject SpawnEnemy(GameObject enemyToSpawn, Vector3 position)
    {
        if (enemyToSpawn == null)
        {
            Debug.LogError("Enemy prefab or EnemySO is null in EnemyManager!");
            return null;
        }

        GameObject spawnedEnemy = Instantiate(enemyToSpawn, position, Quaternion.identity, this.transform);
        Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
        enemy.Initialize();
        return spawnedEnemy;
    }

    public GameObject SpawnEnemyOnCircle(GameObject enemy)
    {
        Vector3 spawnPosition = GetRandomCirclePosition();
        return SpawnEnemy(enemy, spawnPosition);
    }

    public GameObject SpawnEnemyOnCircle(GameObject enemy, float customRadius)
    {
        Vector3 spawnPosition = GetRandomCirclePosition(customRadius);
        return SpawnEnemy(enemy, spawnPosition);
    }

    public GameObject[] SpawnMultipleEnemiesOnCircle(GameObject enemy, int count)
    {
        GameObject[] spawnedEnemies = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            spawnedEnemies[i] = SpawnEnemyOnCircle(enemy);
        }

        return spawnedEnemies;
    }

    public GameObject[] SpawnEnemiesInFormation(GameObject enemy, int count, float radius = -1f)
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

            spawnedEnemies[i] = SpawnEnemy(enemy, spawnPosition);
        }

        return spawnedEnemies;
    }

    private Vector3 GetRandomCirclePosition(float radius = -1f)
    {
        if (radius < 0) radius = spawnRadius;

        float angle = Random.Range(0f, 2f * Mathf.PI);
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
        SerializableDictionary<string, GameObject> enemies = ContentManager.Instance.Enemies;
        foreach (var instance in enemyInstances)
        {
            if (enemies.TryGetValue(instance.id, out GameObject enemyToSpawn))
            {
                GameObject spawnedEnemy = SpawnEnemy(enemyToSpawn, instance.position);
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
                id = enemy.enemyDefaultData.id,
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnCenter, spawnRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(spawnCenter, 0.2f);
    }
#endif

    public void SpawnEnemyWave(StageOfDay stageOfDayCondition)
    {
        if (!enemySpawningConfig.waves.ContainsKey(stageOfDayCondition)) return;
        EnemyWave enemyWave = enemySpawningConfig.waves[stageOfDayCondition];

        foreach (var kvp in enemyWave.enemies.GetAllPairs())
        {
            GameObject enemyPrefab = kvp.Key;
            EnemyWave.EnemySpawnData data = kvp.Value;
            for (int spawnedAmount = 0; spawnedAmount < data.count; spawnedAmount++)
            {
                float angleDeg = data.spawnAngle;
                if (data.count > 1)
                {
                    angleDeg += Random.Range(-20f, 20f);
                }
                float angleRad = angleDeg * Mathf.Deg2Rad;
                Vector3 spawnPosition = spawnCenter + new Vector3(
                    Mathf.Cos(angleRad) * spawnRadius,
                    0f,
                    Mathf.Sin(angleRad) * spawnRadius
                );
                SpawnEnemy(enemyPrefab, spawnPosition);
            }
        }
    }
}