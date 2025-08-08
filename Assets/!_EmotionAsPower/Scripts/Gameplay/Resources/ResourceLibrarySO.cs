using LgTyUtils;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceLibrarySO", menuName = "Scriptable Objects/Resource/ResourceLibrarySO")]
public class ResourceLibrarySO : ScriptableObject
{
    public GameObject[] resources;
    public SerializableDictionary<string, GameObject> resourcePrefabs;

    private void OnValidate()
    {
        // Initialize dictionary if null
        if (resourcePrefabs == null)
        {
            resourcePrefabs = new SerializableDictionary<string, GameObject>();
        }
        else
        {
            resourcePrefabs.Clear();
        }

        // Populate the dictionary with resource prefabs, using Resource.ID as the key
        if (resources != null)
        {
            foreach (var resource in resources)
            {
                if (resource != null)
                {
                    var resourceComponent = resource.GetComponent<Resource>();
                    if (resourceComponent != null && !string.IsNullOrEmpty(resourceComponent.ID))
                    {
                        resourcePrefabs[resourceComponent.ID] = resource;
                    }
                }
            }
        }
    }
}