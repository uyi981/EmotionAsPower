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

    [Header("Projectile Management")]
    [SerializeField]
    public Transform explosivesParent;
    [SerializeField]
    public Transform bulletsParent;

    private void Start()
    {
        DayTimeController.Instance.OnStageOfDayChanged += SpawnEnemyWave;

        // Create parent objects for organization if they don't exist
        if (explosivesParent == null)
        {
            GameObject explosivesContainer = new GameObject("Explosives");
            explosivesContainer.transform.SetParent(this.transform);
            explosivesParent = explosivesContainer.transform;
        }

        if (bulletsParent == null)
        {
            GameObject bulletsContainer = new GameObject("Bullets");
            bulletsContainer.transform.SetParent(this.transform);
            bulletsParent = bulletsContainer.transform;
        }
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
        ClearCurrentProjectiles();

        InitializeLoadedEnemies(gameData.enemies);
        InitializeLoadedExplosives(gameData.explosives);
        InitializeLoadedBullets(gameData.bullets);
    }

    public void SaveGame(ref GameData gameData)
    {
        gameData.enemies = FindAllEnemies();
        gameData.explosives = FindAllExplosives();
        gameData.bullets = FindAllBullets();
    }

    private void ClearCurrentEnemies()
    {
        foreach (Enemy enemy in GetComponentsInChildren<Enemy>())
        {
            Destroy(enemy.gameObject);
        }
    }

    private void ClearCurrentProjectiles()
    {
        // Clear explosives
        if (explosivesParent != null)
        {
            foreach (Transform child in explosivesParent)
            {
                Destroy(child.gameObject);
            }
        }

        // Clear bullets
        if (bulletsParent != null)
        {
            foreach (Transform child in bulletsParent)
            {
                Destroy(child.gameObject);
            }
        }

        // Also clear any explosives/bullets that might be direct children
        foreach (Explosive explosive in GetComponentsInChildren<Explosive>())
        {
            Destroy(explosive.gameObject);
        }

        foreach (Bullet bullet in GetComponentsInChildren<Bullet>())
        {
            Destroy(bullet.gameObject);
        }
    }

    private void InitializeLoadedEnemies(EnemyRuntimeInstance[] enemyInstances)
    {
        if (enemyInstances == null) return;

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

    private void InitializeLoadedExplosives(ExplosiveRuntimeInstance[] explosiveInstances)
    {
        if (explosiveInstances == null) return;

        SerializableDictionary<string, GameObject> explosives = ContentManager.Instance.Explosives;
        foreach (var instance in explosiveInstances)
        {
            if (explosives.TryGetValue(instance.id, out GameObject explosiveToSpawn))
            {
                GameObject spawnedExplosive = Instantiate(explosiveToSpawn, instance.position, instance.rotation, explosivesParent);
                Explosive explosive = spawnedExplosive.GetComponent<Explosive>();

                if (explosive != null)
                {
                    explosive.explosionRange = instance.explosionRange;
                    explosive.explosionDamage = instance.explosionDamage;
                    explosive.damageLayerMask = instance.damageLayerMask;
                    explosive.currentTimer = instance.currentTimer;
                }
            }
            else
            {
                Debug.LogWarning($"Explosive prefab with ID {instance.id} not found.");
            }
        }
    }

    private void InitializeLoadedBullets(BulletRuntimeInstance[] bulletInstances)
    {
        if (bulletInstances == null) return;

        SerializableDictionary<string, GameObject> bullets = ContentManager.Instance.Bullets;
        foreach (var instance in bulletInstances)
        {
            if (bullets.TryGetValue(instance.id, out GameObject bulletToSpawn))
            {
                GameObject spawnedBullet = Instantiate(bulletToSpawn, instance.position, Quaternion.LookRotation(instance.direction), bulletsParent);
                Bullet bullet = spawnedBullet.GetComponent<Bullet>();

                if (bullet != null)
                {
                    bullet.Initialize(instance.direction, instance.damage, instance.speed, instance.remainingLifetime, instance.damageLayerMask);
                    bullet.currentLifetime = bullet.lifetime - instance.remainingLifetime;
                }
            }
            else
            {
                Debug.LogWarning($"Bullet prefab with ID {instance.id} not found.");
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

    private ExplosiveRuntimeInstance[] FindAllExplosives()
    {
        Explosive[] explosives = GetComponentsInChildren<Explosive>();
        List<ExplosiveRuntimeInstance> result = new List<ExplosiveRuntimeInstance>();

        foreach (Explosive explosive in explosives)
        {
            string explosiveId = GetPrefabId(explosive.gameObject, "explosive");

            if (!string.IsNullOrEmpty(explosiveId))
            {
                result.Add(new ExplosiveRuntimeInstance
                {
                    id = explosiveId,
                    position = explosive.transform.position,
                    rotation = explosive.transform.rotation,
                    explosionRange = explosive.explosionRange,
                    explosionDamage = explosive.explosionDamage,
                    damageLayerMask = explosive.damageLayerMask,
                    hasExploded = false,
                    currentTimer = explosive.currentTimer
                });
            }
        }

        return result.ToArray();
    }

    private BulletRuntimeInstance[] FindAllBullets()
    {
        Bullet[] bullets = GetComponentsInChildren<Bullet>();
        List<BulletRuntimeInstance> result = new List<BulletRuntimeInstance>();

        foreach (Bullet bullet in bullets)
        {
            string bulletId = GetPrefabId(bullet.gameObject, "bullet");

            if (!string.IsNullOrEmpty(bulletId))
            {
                result.Add(new BulletRuntimeInstance
                {
                    id = bulletId,
                    position = bullet.transform.position,
                    direction = bullet.direction,
                    damage = bullet.damage,
                    speed = bullet.speed,
                    remainingLifetime = bullet.lifetime - bullet.currentLifetime,
                    damageLayerMask = bullet.damageLayerMask
                });
            }
        }

        return result.ToArray();
    }

    private string GetPrefabId(GameObject instance, string type)
    {
        string instanceName = instance.name.Replace("(Clone)", "").Trim();
        SerializableDictionary<string, GameObject> dict = type switch
        {
            "explosive" => ContentManager.Instance.Explosives,
            "bullet" => ContentManager.Instance.Bullets,
            _ => null
        };

        if (dict == null) return null;

        foreach (var kv in dict)
        {
            if (kv.Value != null && kv.Value.name == instanceName)
            {
                return kv.Key;
            }
        }

        Debug.LogWarning($"No prefab found for {instanceName} in {type}s");
        return null;
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

        // Iterate through each enemy type and their spawn data arrays
        foreach (var enemyEntry in enemyWave.enemies.Dictionary)
        {
            GameObject enemyPrefab = enemyEntry.Key;
            EnemyWave.EnemySpawnData[] spawnDataArray = enemyEntry.Value;

            // Process each spawn data configuration for this enemy type
            foreach (EnemyWave.EnemySpawnData spawnData in spawnDataArray)
            {
                // Spawn the specified count of enemies for this configuration
                for (int spawnedAmount = 0; spawnedAmount < spawnData.count; spawnedAmount++)
                {
                    float angleDeg = spawnData.spawnAngle;

                    // Add random variation if spawning multiple enemies
                    if (spawnData.count > 1)
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

    public void ClearAllThreats()
    {
        // Clear all enemies
        foreach (Enemy enemy in GetComponentsInChildren<Enemy>())
        {
            Destroy(enemy.gameObject);
        }

        // Clear all explosives
        if (explosivesParent != null)
        {
            foreach (Transform child in explosivesParent)
            {
                Destroy(child.gameObject);
            }
        }

        // Clear all bullets
        if (bulletsParent != null)
        {
            foreach (Transform child in bulletsParent)
            {
                Destroy(child.gameObject);
            }
        }

        // Ensure any stray explosives or bullets are cleared
        foreach (Explosive explosive in GetComponentsInChildren<Explosive>())
        {
            Destroy(explosive.gameObject);
        }

        foreach (Bullet bullet in GetComponentsInChildren<Bullet>())
        {
            Destroy(bullet.gameObject);
        }
    }
}