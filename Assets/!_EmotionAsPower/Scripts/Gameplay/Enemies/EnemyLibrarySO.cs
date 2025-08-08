using LgTyUtils;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyLibrarySO", menuName = "Scriptable Objects/Enemy/EnemyLibrarySO")]
public class EnemyLibrarySO : ScriptableObject
{
    public GameObject[] enemies;
    public SerializableDictionary<string, GameObject> enemyPrefabs;

    private void OnValidate()
    {
        // Initialize dictionary if null
        if (enemyPrefabs == null)
        {
            enemyPrefabs = new SerializableDictionary<string, GameObject>();
        }
        else
        {
            enemyPrefabs.Clear();
        }

        // Populate the dictionary with enemy prefabs, using Enemy.ID as the key
        if (enemies != null)
        {
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    var enemyComponent = enemy.GetComponent<Enemy>();
                    if (enemyComponent != null && !string.IsNullOrEmpty(enemyComponent.enemyDefaultData.id))
                    {
                        enemyPrefabs[enemyComponent.enemyDefaultData.id] = enemy;
                    }
                }
            }
        }
    }
}