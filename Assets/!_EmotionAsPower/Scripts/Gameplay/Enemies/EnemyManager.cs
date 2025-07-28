using System.Collections.Generic;
using System.Linq;
using LgTyUtils;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyManager : Singleton<EnemyManager>, IDataPersistence
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private Vector3 spawnCenter = Vector3.zero;

    [Header("AI Behavior Settings")]
    [SerializeField] private AIBehaviour defaultEnemyBehavior;
    [SerializeField] private AIBehaviour aggressiveEnemyBehavior;
    [SerializeField] private AIBehaviour defensiveEnemyBehavior;

    [Header("Performance Settings")]
    [SerializeField] private int maxActiveEnemies = 50;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    private List<Enemy> activeEnemies = new List<Enemy>();
    private Dictionary<string, AIBehaviour> behaviorLibrary = new Dictionary<string, AIBehaviour>();

    public System.Action<Enemy> OnEnemySpawned;
    public System.Action<Enemy> OnEnemyDestroyed;
    public System.Action<int> OnEnemyCountChanged;

    public int ActiveEnemyCount => activeEnemies.Count;
    public List<Enemy> ActiveEnemies => new List<Enemy>(activeEnemies);

    protected override void Awake()
    {
        base.Awake();
        InitializeBehaviorLibrary();
    }

    private void Start()
    {

    }

    private void InitializeBehaviorLibrary()
    {
        // Register common behaviors
        if (defaultEnemyBehavior != null)
            behaviorLibrary["default"] = defaultEnemyBehavior;
        if (aggressiveEnemyBehavior != null)
            behaviorLibrary["aggressive"] = aggressiveEnemyBehavior;
        if (defensiveEnemyBehavior != null)
            behaviorLibrary["defensive"] = defensiveEnemyBehavior;
    }

    public GameObject SpawnEnemy(EnemySO enemySO, Vector3 position, string behaviorType = "default")
    {
        if (enemyPrefab == null || enemySO == null)
        {
            Debug.LogError("Enemy prefab or EnemySO is null in EnemyManager!");
            return null;
        }

        // Check if we're at max capacity
        if (activeEnemies.Count >= maxActiveEnemies)
        {
            if (debugMode)
                Debug.LogWarning("Maximum enemy limit reached. Cannot spawn more enemies.");
            return null;
        }

        // Instantiate enemy
        GameObject spawnedEnemy = Instantiate(enemyPrefab, position, Quaternion.identity, this.transform);
        Enemy enemy = spawnedEnemy.GetComponent<Enemy>();

        if (enemy == null)
        {
            Debug.LogError("Enemy prefab doesn't have Enemy component!");
            Destroy(spawnedEnemy);
            return null;
        }

        // Initialize enemy with data
        enemy.Initialize(enemySO);

        // Set up AI behavior
        SetupEnemyBehavior(enemy, behaviorType);

        // Register enemy
        RegisterEnemy(enemy);

        if (debugMode)
            Debug.Log($"Spawned enemy {enemy.name} at {position} with behavior {behaviorType}");

        return spawnedEnemy;
    }

    public GameObject SpawnEnemyOnCircle(EnemySO enemySO, string behaviorType = "default")
    {
        Vector3 spawnPosition = GetRandomCirclePosition();
        return SpawnEnemy(enemySO, spawnPosition, behaviorType);
    }

    public GameObject SpawnEnemyOnCircle(EnemySO enemySO, float customRadius, string behaviorType = "default")
    {
        Vector3 spawnPosition = GetRandomCirclePosition(customRadius);
        return SpawnEnemy(enemySO, spawnPosition, behaviorType);
    }

    public GameObject[] SpawnMultipleEnemiesOnCircle(EnemySO enemySO, int count, string behaviorType = "default")
    {
        List<GameObject> spawnedEnemies = new List<GameObject>();

        for (int i = 0; i < count && activeEnemies.Count < maxActiveEnemies; i++)
        {
            GameObject enemy = SpawnEnemyOnCircle(enemySO, behaviorType);
            if (enemy != null)
                spawnedEnemies.Add(enemy);
        }

        return spawnedEnemies.ToArray();
    }

    public GameObject[] SpawnEnemiesInFormation(EnemySO enemySO, int count, float radius = -1f, string behaviorType = "default")
    {
        if (radius < 0) radius = spawnRadius;

        List<GameObject> spawnedEnemies = new List<GameObject>();
        float angleStep = 360f / count;

        for (int i = 0; i < count && activeEnemies.Count < maxActiveEnemies; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 spawnPosition = spawnCenter + new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            GameObject enemy = SpawnEnemy(enemySO, spawnPosition, behaviorType);
            if (enemy != null)
                spawnedEnemies.Add(enemy);
        }

        return spawnedEnemies.ToArray();
    }

    public void SpawnWave(EnemyWaveConfig waveConfig)
    {
        if (waveConfig == null) return;

        foreach (var spawnGroup in waveConfig.spawnGroups)
        {
            for (int i = 0; i < spawnGroup.count; i++)
            {
                Vector3 spawnPos = waveConfig.useFormation ?
                    GetFormationPosition(i, spawnGroup.count, waveConfig.formationRadius) :
                    GetRandomCirclePosition(waveConfig.spawnRadius);

                SpawnEnemy(spawnGroup.enemySO, spawnPos, spawnGroup.behaviorType);

                // Add delay
                if (spawnGroup.spawnDelay > 0)
                {
                    StartCoroutine(DelayedSpawn(spawnGroup, waveConfig, i + 1));
                    break; 
                }
            }
        }
    }

    private System.Collections.IEnumerator DelayedSpawn(EnemySpawnGroup spawnGroup, EnemyWaveConfig waveConfig, int startIndex)
    {
        for (int i = startIndex; i < spawnGroup.count; i++)
        {
            yield return new WaitForSeconds(spawnGroup.spawnDelay);

            Vector3 spawnPos = waveConfig.useFormation ?
                GetFormationPosition(i, spawnGroup.count, waveConfig.formationRadius) :
                GetRandomCirclePosition(waveConfig.spawnRadius);

            SpawnEnemy(spawnGroup.enemySO, spawnPos, spawnGroup.behaviorType);
        }
    }

    private void SetupEnemyBehavior(Enemy enemy, string behaviorType)
    {
        if (behaviorLibrary.TryGetValue(behaviorType, out AIBehaviour behavior))
        {
            enemy.AIController.SetBehavior(behavior);
        }
        else if (defaultEnemyBehavior != null)
        {
            enemy.AIController.SetBehavior(defaultEnemyBehavior);
            if (debugMode)
                Debug.LogWarning($"Behavior type '{behaviorType}' not found. Using default behavior.");
        }
    }

    private void RegisterEnemy(Enemy enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
            enemy.OnEnemyDeath += OnEnemyDied;

            OnEnemySpawned?.Invoke(enemy);
            OnEnemyCountChanged?.Invoke(activeEnemies.Count);
        }
    }

    private void OnEnemyDied(Enemy enemy)
    {
        UnregisterEnemy(enemy);
    }

    private void UnregisterEnemy(Enemy enemy)
    {
        if (activeEnemies.Remove(enemy))
        {
            enemy.OnEnemyDeath -= OnEnemyDied;

            OnEnemyDestroyed?.Invoke(enemy);
            OnEnemyCountChanged?.Invoke(activeEnemies.Count);
        }
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

    private Vector3 GetFormationPosition(int index, int totalCount, float radius)
    {
        float angleStep = 360f / totalCount;
        float angle = index * angleStep * Mathf.Deg2Rad;

        return spawnCenter + new Vector3(
            Mathf.Cos(angle) * radius,
            0f,
            Mathf.Sin(angle) * radius
        );
    }

    public void SetSpawnParameters(float radius, Vector3 center)
    {
        spawnRadius = radius;
        spawnCenter = center;
    }

    public void RegisterBehavior(string key, AIBehaviour behavior)
    {
        behaviorLibrary[key] = behavior;
    }

    public void ChangeAllEnemiesBehavior(string behaviorType)
    {
        if (!behaviorLibrary.TryGetValue(behaviorType, out AIBehaviour behavior))
        {
            Debug.LogWarning($"Behavior type '{behaviorType}' not found.");
            return;
        }

        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && enemy.AIController != null)
            {
                enemy.AIController.SetBehavior(behavior);
            }
        }
    }

    public void DestroyAllEnemies()
    {
        var enemiestoDestroy = new List<Enemy>(activeEnemies);

        foreach (var enemy in enemiestoDestroy)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        activeEnemies.Clear();
        OnEnemyCountChanged?.Invoke(0);
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
            UnregisterEnemy(enemy);
            Destroy(enemy.gameObject);
        }
        activeEnemies.Clear();
    }

    private void InitializeLoadedEnemies(EnemyRuntimeInstance[] enemyInstances)
    {
        SerializableDictionary<string, EnemySO> enemySOs = ContentManager.Instance.EnemySOs;

        foreach (var instance in enemyInstances)
        {
            if (enemySOs.TryGetValue(instance.id, out EnemySO enemySO))
            {
                GameObject spawnedEnemy = SpawnEnemy(enemySO, instance.position);
                if (spawnedEnemy != null)
                {
                    Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
                    enemy.Health.SetHealth(instance.currentHealth);
                    enemy.UpdateExistingTime(instance.remainExistingTime);
                }
            }
            else
            {
                Debug.LogWarning($"EnemySO with ID {instance.id} not found.");
            }
        }
    }

    private EnemyRuntimeInstance[] FindAllEnemies()
    {
        List<EnemyRuntimeInstance> result = new List<EnemyRuntimeInstance>();

        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy != null && enemy.EnemySO != null)
            {
                result.Add(new EnemyRuntimeInstance
                {
                    id = enemy.EnemySO.ID,
                    position = enemy.transform.position,
                    currentHealth = enemy.Health.CurrentHealth,
                    remainExistingTime = enemy.ExistingTimer
                });
            }
        }

        return result.ToArray();
    }

    private void OnDrawGizmosSelected()
    {
        // Draw spawn circle
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnCenter, spawnRadius);

        // Draw center point
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(spawnCenter, 0.2f);

        // Draw enemy count info
#if UNITY_EDITOR
        UnityEditor.Handles.Label(spawnCenter + Vector3.up * 2f, $"Active Enemies: {activeEnemies.Count}/{maxActiveEnemies}");
#endif
    }
}

