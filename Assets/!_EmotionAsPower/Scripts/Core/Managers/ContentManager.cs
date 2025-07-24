using LgTyUtils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ContentManager : Singleton<ContentManager>
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
    protected override void Awake()
    {
        base.Awake();
        itemSOs = LoadlAllContentsOfType<ItemSO>(debugLoading);
        resourceSOs = LoadlAllContentsOfType<ResourceSO>(debugLoading);
        enemySOs = LoadlAllContentsOfType<EnemySO>(debugLoading);
    }

    public SerializableDictionary<string, T> LoadlAllContentsOfType<T>(bool debug) where T : BaseScriptableObject
    {
        SerializableDictionary<string, T> result = new SerializableDictionary<string, T>(); 
        Addressables.LoadAssetsAsync<T>(typeof(T).Name, item =>
        {
            if (item != null)
            {
                result.Add(item.ID, item);
            }
        }).Completed += handled =>
        {
            if (handled.Status == AsyncOperationStatus.Succeeded)
            {
                if (debug)
                {
                    Debug.Log($"Successfully loaded {itemSOs.Count} ItemSO assets");
                }
            }
            else
            {
                Debug.LogError($"Failed to load ItemSO assets: {handled.OperationException?.Message}");
            }
        };
        return result;
    }

    // TODO: Implement loading and unloading of a specific amount of content using pooling
    // Luong: Currently have no idea how to do this
}