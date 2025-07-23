using System;
using LgTyUtils;
using UnityEngine;

public class ResourceManager : Singleton<ResourceManager>, IDataPersistence
{
    [SerializeField]
    private GameObject resourcePrefab;

    /// <summary>
    /// Loads resource data from the game data, clears existing resources, and spawns them with saved states.
    /// </summary>
    public void LoadGame(GameData gameData)
    {
        ClearCurrentResources();
        InitializeLoadedResources(gameData.resources);
    }

    /// <summary>
    /// Saves the current state of all resources in the scene to the game data.
    /// </summary>
    public void SaveGame(ref GameData gameData)
    {
        gameData.resources = FindAllResources();
    }

    /// <summary>
    /// Spawns a resource at the specified position with the given ResourceSO.
    /// </summary>
    /// <returns>The spawned resource GameObject.</returns>
    public GameObject SpawnResource(ResourceSO resourceSO, Vector3 position)
    {
        GameObject spawnedResource = Instantiate(resourcePrefab, position, resourcePrefab.transform.rotation, this.transform);
        Resource resource = spawnedResource.GetComponent<Resource>();
        resource.Initialize(resourceSO);
        return spawnedResource;
    }

    /// <summary>
    /// Initializes resources from saved data, spawning them and setting their health.
    /// </summary>
    private void InitializeLoadedResources(ResourceRuntimeInstance[] resourceInstances)
    {
        SerializableDictionary<string, ResourceSO> resourceSOs = ContentManager.Instance.ResourceSOs;
        foreach (var resourceInstance in resourceInstances)
        {
            if (resourceSOs.TryGetValue(resourceInstance.id, out var resourceSO))
            {
                GameObject spawnedResource = SpawnResource(resourceSO, resourceInstance.position);
                Resource resource = spawnedResource.GetComponent<Resource>();
                resource.Health.SetHealth(resourceInstance.health);
            }
            else
            {
                Debug.LogWarning($"ResourceSO with ID {resourceInstance.id} not found.");
            }
        }
    }

    /// <summary>
    /// Finds all resources in the scene and collects their runtime data.
    /// </summary>
    private ResourceRuntimeInstance[] FindAllResources()
    {
        var resources = GetComponentsInChildren<Resource>();
        ResourceRuntimeInstance[] result = new ResourceRuntimeInstance[resources.Length];
        for (int i = 0; i < resources.Length; i++)
        {
            var resource = resources[i];
            result[i] = new ResourceRuntimeInstance(resource.ResourceSO.ID, resource.transform.position, resource.Health.CurrentHealth);
        }
        return result;
    }

    /// <summary>
    /// Destroys all resource GameObjects currently managed by this instance.
    /// </summary>
    private void ClearCurrentResources()
    {
        foreach (Resource resource in GetComponentsInChildren<Resource>())
        {
            Destroy(resource.gameObject);
        }
    }
}