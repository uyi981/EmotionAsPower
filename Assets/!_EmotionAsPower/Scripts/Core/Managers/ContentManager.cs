using System.Collections;
using LgTyUtils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ContentManager : Singleton<ContentManager>, ISetup
{
    [SerializeField]
    private bool debugLoading = true;   
    [SerializeField]
    private SerializableDictionary<string, ItemSO> itemSOs;
    public SerializableDictionary<string, ItemSO> ItemSOs => itemSOs;

    [SerializeField]
    private SerializableDictionary<string, ResourceSO> resourceSOs;
    public SerializableDictionary<string, ResourceSO> ResourceSOs => resourceSOs;

    [SerializeField]
    private SerializableDictionary<string, EnemySO> enemySOs;
    public SerializableDictionary<string, EnemySO> EnemySOs => enemySOs;

    public IEnumerator LoadAllContentsOfTypeCoroutine<T>(SerializableDictionary<string, T> dictionary, bool debug) where T : BaseScriptableObject
    {
        string label = typeof(T).Name;
        Debug.Log($"Attempting to load assets with label: {label}");

        var handle = Addressables.LoadAssetsAsync<T>(label, item =>
        {
            if (item != null)
            {
                dictionary.Add(item.ID, item);
                if (debug)
                {
                    Debug.Log($"Loaded: {item.ID} ({typeof(T).Name})");
                }
            }
            else
            {
                Debug.LogWarning($"Null item found when loading {typeof(T).Name}");
            }
        });

        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            if (debug)
            {
                Debug.Log($"Successfully loaded {dictionary.Count} {typeof(T).Name} assets");
            }
        }
        else
        {
            Debug.LogError($"Failed to load {typeof(T).Name} assets: {handle.OperationException?.Message}");
        }
    }

    public void Setup()
    {
        StartCoroutine(SetupCoroutine());
    }
    public IEnumerator SetupCoroutine()
    {
        Debug.Log("ContentManager Setup Started");

        // Initialize dictionaries
        itemSOs = new SerializableDictionary<string, ItemSO>();
        resourceSOs = new SerializableDictionary<string, ResourceSO>();
        enemySOs = new SerializableDictionary<string, EnemySO>();

        Debug.Log("Starting to load content...");

        var loadItems = StartCoroutine(LoadAllContentsOfTypeCoroutine<ItemSO>(itemSOs, debugLoading));
        var loadResources = StartCoroutine(LoadAllContentsOfTypeCoroutine<ResourceSO>(resourceSOs, debugLoading));
        var loadEnemies = StartCoroutine(LoadAllContentsOfTypeCoroutine<EnemySO>(enemySOs, debugLoading));

        yield return loadItems;
        yield return loadResources;
        yield return loadEnemies;

        Debug.Log("ContentManager Setup Completed");
    }

    // TODO: Implement loading and unloading of a specific amount of content using pooling
    // Luong: Currently have no idea how to do this
}