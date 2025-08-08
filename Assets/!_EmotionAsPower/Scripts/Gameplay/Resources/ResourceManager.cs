using System;
using LgTyUtils;
using UnityEngine;

public class ResourceManager : Singleton<ResourceManager>, IDataPersistence
{
    [SerializeField]
    private GameObject resourcePrefab;

    public void LoadGame(GameData gameData)
    {
        ClearCurrentResources();
        InitializeLoadedResources(gameData.resources);
    }

    public void SaveGame(ref GameData gameData)
    {
        gameData.resources = FindAllResources();
    }

    public GameObject SpawnResource(ResourceSO resourceSO, Vector3 position)
    {
        GameObject spawnedResource = Instantiate(resourcePrefab, position, resourcePrefab.transform.rotation, this.transform);
        Resource resource = spawnedResource.GetComponent<Resource>();
        resource.Initialize(resourceSO);
        return spawnedResource;
    }

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

    private void ClearCurrentResources()
    {
        foreach (Resource resource in GetComponentsInChildren<Resource>())
        {
            Destroy(resource.gameObject);
        }
    }
}