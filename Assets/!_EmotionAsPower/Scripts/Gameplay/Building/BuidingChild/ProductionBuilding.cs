using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Assets.__EmotionAsPower.Scripts.UI.ProcessBar;

public class ProductionBuilding : BuildingBase, IProductionBuilding
{
    [Tooltip("Prefab của thanh tiến độ sản xuất")]
    [SerializeField] private GameObject productionBarPrefab;


    [Header("Production Settings")]
    [SerializeField] private float productionTime = 5f;
    [SerializeField] private float currentProductionTime = 0f;
    [SerializeField] private bool isProducing = false;



    [Header("Input/Output")]
    [SerializeField] private List<ItemSO> inputItems;
    [SerializeField] private List<ItemSO> outputItems;
    [SerializeField] private int productionAmount = 1;


    
    [Tooltip("Emotion yêu cầu để sản xuất")]
    [SerializeField] public Emotion requireEmotion = Emotion.Normal;



    private GameObject productionBarInstance;



    public bool IsProducing => isProducing;

    private void Awake()
    {
        if (itemDropper == null)
        {
            itemDropper = GetComponent<ItemDropper>();
            if (itemDropper == null)
            {
                Debug.LogWarning("No ItemDropper found on ProductionBuilding. Item drops will not work.");
            }
        }
    }

  



    public override void Start()
    {
        base.Start();
        StartProduction();
    }


    


    public void StartProduction()
    {
        productionBarInstance = Instantiate(productionBarPrefab, transform.position + Vector3.up * 1f, Quaternion.identity, transform);
        productionBarInstance.SetActive(false); // Ẩn thanh tiến độ sản xuất ban đầu
    }

    public void StopProduction()
    {
        if (isProducing)
        {
            isProducing = false;
            base.ResetWorkerList();
            workers.Clear();
            workersAmount = 0;
        }
    }

    public IEnumerator ProduceItem()
    {
        while (isProducing)
        {
            productionBarInstance.SetActive(true);
            Debug.Log($"{Name} đang sản xuất...");

            // Update production progress
            currentProductionTime = 0f;
            while (currentProductionTime < productionTime && isProducing)
            {
                currentProductionTime += 0.1f;
                ProcessBar processBar = productionBarInstance.GetComponent<ProcessBar>();

                processBar.SetProcess(currentProductionTime / productionTime);

                yield return new WaitForSeconds(0.1f);
            }

            CompleteProduction();
        }
    }


    public override void OnBuildingComplete()
    {
        base.OnBuildingComplete();
        isProducing = true;
        Debug.Log($" đã hoàn thành!");
        base.AssignJobToWorker(JobType.Produce);
        StartCoroutine(ProduceItem());
    }

    private void CompleteProduction()
    {
        // Tiêu thụ nguyên liệu đầu vào
        if (!ConsumeInputs())
        {
            return;
        }

        // Tạo sản phẩm
        if (itemDropper != null)
        {
            // Dùng ItemManager.Instance.SpawnItem spawn mang outputItems
            ItemManager.Instance.SpawnItem(outputItems[0], productionAmount, transform.position + new Vector3(0, 0.5f, -(1 + selectedBuilding.size.y)));
            Vector3Int vector3Int = Singleton<GridSystem>.Instance.grid.WorldToCell(transform.position + new Vector3(0, 0.5f, -(1 + selectedBuilding.size.y)));
            Vector2Int vector2Int = new Vector2Int(vector3Int.x, vector3Int.z);
            Singleton<VillagerManager>.Instance.SendJobRequestToManager(new JobForWorker(vector2Int, JobType.Transport, this));
            Debug.Log($"{Name} đã sản xuất {productionAmount} sản phẩm!");
            productionBarInstance.SetActive(false); // Ẩn thanh tiến độ sản xuất sau khi hoàn thành
        }

    }

    private bool ConsumeInputs()
    {
        foreach (var item in inputItems)
        {
            int amount = Singleton<ItemStorage>.Instance.TryTakeItem(item.ID, 10);
            if (amount <= 0)
            {
                Debug.LogWarning($"Không đủ nguyên liệu!");
                StopProduction();
                return false;
            }
        }
        return true;
        // Tiêu thụ nguyên liệu đầu vào
        // Cần implement InventorySystem để xử lý
    }

    public override void OnBuildingDestroyed()
    {
        base.OnBuildingDestroyed();
        StopProduction();
    }

   

   
}
