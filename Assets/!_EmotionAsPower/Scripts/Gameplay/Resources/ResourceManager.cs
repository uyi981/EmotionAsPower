using System;
using LgTyUtils;
using UnityEngine;
using Random = UnityEngine.Random;

public class ResourceManager : Singleton<ResourceManager>, IDataPersistence
{
    [SerializeField]
    private float spawnResourceRegularRange = 50f;

    [Serializable]
    public class ResourceSpawnConfig
    {
        public float chance;
        public float maxAmount;
    }
    [SerializeField]
    private SerializableDictionary<GameObject, ResourceSpawnConfig> resourcesForRegularSpawning;

    public void FixedUpdate()
    {
        float randomValue = Random.value;
        foreach(var kvp in resourcesForRegularSpawning)
        {
            if(kvp.Value.chance >= randomValue)
            {
                if(GetAmountOfSameResource(kvp.Key.GetComponent<Resource>())  < kvp.Value.maxAmount)
                SpawnResource(kvp.Key, GetSpawnPositionInSpawnRange());
            }
        }
    }

    public int GetAmountOfSameResource(Resource resourceToCheck)
    {
        var resources = FindAllResources();
        int amount = 0;
        foreach (var resource in resources)
        {
            if(resource.id == resourceToCheck.ID)
            {
                amount++;
            }
        }
        return amount;
    }
    public void LoadGame(GameData gameData)
    {
        ClearCurrentResources();
        InitializeLoadedResources(gameData.resources);
    }

    public void SaveGame(ref GameData gameData)
    {
        gameData.resources = FindAllResources();
    }

    public GameObject SpawnResource(GameObject resourceToSpawn, Vector3 position)
    {
        GameObject spawnedResource = Instantiate(resourceToSpawn, position, resourceToSpawn.transform.rotation, this.transform);
        Resource resource = spawnedResource.GetComponent<Resource>();
        resource.Initialize();
        return spawnedResource;
    }

    public GameObject SpawnResourceWithSaveData(GameObject resourceToSpawn, Vector3 position, float health, float lastDropTime, bool isForHarvest = false)
    {
        GameObject spawnedResource = Instantiate(resourceToSpawn, position, resourceToSpawn.transform.rotation, this.transform);
        Resource resource = spawnedResource.GetComponent<Resource>();
        resource.InitializeWithSaveData(health, lastDropTime, isForHarvest);
        return spawnedResource;
    }

    private void InitializeLoadedResources(ResourceRuntimeInstance[] resourceInstances)
    {
        SerializableDictionary<string, GameObject> resources = ContentManager.Instance.Resources;

        foreach (var resourceInstance in resourceInstances)
        {
            if (resources.TryGetValue(resourceInstance.id, out var resourcePrefab))
            {
                GameObject spawnedResource = SpawnResourceWithSaveData(
                    resourcePrefab,
                    resourceInstance.position,
                    resourceInstance.health,
                    resourceInstance.lastRegularDropTime,
                    resourceInstance.isForHarvest
                );
            }
            else
            {
                Debug.LogWarning($"ResourceSO with ID {resourceInstance.id} not found.");
            }
        }
    }

    private ResourceRuntimeInstance[] FindAllResources()
    {
        var resources = GetComponentsInChildren<Resource>();
        ResourceRuntimeInstance[] result = new ResourceRuntimeInstance[resources.Length];

        for (int i = 0; i < resources.Length; i++)
        {
            var resource = resources[i];
            result[i] = new ResourceRuntimeInstance(
                resource.ID,
                resource.transform.position,
                resource.Health.CurrentHealth,
                resource.LastRegularDropTime,
                resource.IsForHarvest
            );
        }

        return result;
    }

    private void ClearCurrentResources()
    {
        foreach (Resource resource in GetComponentsInChildren<Resource>())
        {
            Destroy(resource.gameObject);
        }
    }

    // Utility methods for managing resources at runtime
    public void EnableRegularDropsForAll(bool enable)
    {
        foreach (Resource resource in GetComponentsInChildren<Resource>())
        {
            resource.EnableRegularDrops(enable);
        }
    }

    public void SetRegularDropIntervalForAll(float interval)
    {
        foreach (Resource resource in GetComponentsInChildren<Resource>())
        {
            resource.SetRegularDropInterval(interval);
        }
    }

    public void ForceRegularDropForAll()
    {
        foreach (Resource resource in GetComponentsInChildren<Resource>())
        {
            resource.ForceRegularDrop();
        }
    }

    public Resource[] GetAllResources()
    {
        return GetComponentsInChildren<Resource>();
    }

    public Resource GetResourceById(string id)
    {
        foreach (Resource resource in GetComponentsInChildren<Resource>())
        {
            if (resource.ID == id)
                return resource;
        }
        return null;
    }

    public void SetResourceForHarvest(string resourceId)
    {
        Resource resource = GetResourceById(resourceId);
        if (resource != null)
        {
            resource.SetForHarvest();
        }
    }

    public void UnsetResourceForHarvest(string resourceId)
    {
        Resource resource = GetResourceById(resourceId);
        if (resource != null)
        {
            resource.UnsetForHarvest();
        }
    }

    public void SetAllResourcesForHarvest()
    {
        foreach (Resource resource in GetComponentsInChildren<Resource>())
        {
            resource.SetForHarvest();
        }
    }

    public void UnsetAllResourcesForHarvest()
    {
        foreach (Resource resource in GetComponentsInChildren<Resource>())
        {
            resource.UnsetForHarvest();
        }
    }

    public Resource[] GetHarvestableResources()
    {
        return System.Array.FindAll(GetComponentsInChildren<Resource>(), r => r.IsForHarvest);
    }

    public Resource[] GetNonHarvestableResources()
    {
        return System.Array.FindAll(GetComponentsInChildren<Resource>(), r => !r.IsForHarvest);
    }

    public Vector3 GetSpawnPositionInSpawnRange()
    {
        Vector3 point = Random.insideUnitSphere * spawnResourceRegularRange;
        point.y = 0;
        return point;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, spawnResourceRegularRange);
    }
}