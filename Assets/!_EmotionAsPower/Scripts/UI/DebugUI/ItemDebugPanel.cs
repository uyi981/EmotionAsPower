using System.Linq;
using UnityEngine;

public class ItemDebugPanel : MonoBehaviour
{
    public bool forEmotion = true;
    [SerializeField]
    private Transform content;
    [SerializeField]
    private GameObject itemInStoragePrefab;

    private void Start()
    {
        GameManager.Instance.OnSetupFinished += () => Initialize();
    }

    public void Initialize()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        ItemSO[] itemSOs = ContentManager.Instance.ItemSOs.Values.ToArray();
        foreach (var itemSO in itemSOs)
        {
            if (forEmotion == (itemSO.category == ItemCategory.Emotion))
            {
                GameObject itemObj = Instantiate(itemInStoragePrefab, content);

                ItemUISlotForDebug itemInStorage = itemObj.GetComponent<ItemUISlotForDebug>();
                if (itemInStorage != null)
                {
                    itemInStorage.SetData(itemSO, 100);
                }
            }
        }

    }
}