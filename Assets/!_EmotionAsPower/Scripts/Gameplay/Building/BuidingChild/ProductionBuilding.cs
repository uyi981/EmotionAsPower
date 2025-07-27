using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ProductionBuilding : BuildingBase, IProductionBuilding
{
    [Header("Production Settings")]
    [SerializeField] private float productionRate = 1f;
    [SerializeField] private float productionTime = 5f;
    [SerializeField] private float currentProductionTime = 0f;
    [SerializeField] private bool isProducing = false;
    
    [Header("Item Drop")]
    [SerializeField] private ItemDropper itemDropper;
    [SerializeField] private bool dropItemsOnProduction = true;
    
    [Header("Input/Output")]
    [SerializeField] private List<ItemSO> inputItems;
    [SerializeField] private List<ItemSO> outputItems;
    [SerializeField] private int productionAmount = 1;


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

    public bool IsProducing => isProducing;

    public float ProductionRate => productionRate;

    public string Name => gameObject.name;

    public override void Start()
    {
        base.Start();
    }

    public override void OnBuildingComplete()
    {

        ResetWorkerList();
        isProducing = true;
        Debug.Log($" đã hoàn thành!");
        base.AssignJobToWorker(JobType.Produce);
        StartCoroutine(ProduceItem());
    }
    private void UpdateProduction()
    {
        //if (!HasRequiredInputs())
        //{
        //    StopProduction();
        //    return;
        //}

        currentProductionTime += Time.deltaTime * productionRate;
        
        if (currentProductionTime >= productionTime)
        {
            CompleteProduction();
            currentProductionTime = 0f;
        }
    }
    IEnumerator ProduceItem()
    {
        while(isProducing)
        {
            Debug.Log($"{Name} đang sản xuất...");
            yield return new WaitForSeconds(productionTime);
            CompleteProduction();
        }
    }
    private bool HasRequiredInputs()
    {
        return true; // Tạm thởi luôn trả về true
        return true; // Tạm thời luôn trả về true
    }
    private void CompleteProduction()
    {
        // Tiêu thụ nguyên liệu đầu vào
        if(!ConsumeInputs())
        {
            return;
        }    
        
        // Tạo sản phẩm
        if (dropItemsOnProduction && itemDropper != null)
        {
            // Dùng ItemManager.Instance.SpawnItem spawn mang outputItems
                ItemManager.Instance.SpawnItem(outputItems[0], productionAmount, transform.position+ new Vector3(0,0.5f, -(1 + selectedBuilding.size.y)));
                Vector3Int vector3Int = Singleton<GridSystem>.Instance.grid.WorldToCell(transform.position + new Vector3(0, 0.5f, -(1 + selectedBuilding.size.y)));
                Vector2Int vector2Int = new Vector2Int(vector3Int.x, vector3Int.z);
                Singleton<VillagerManager>.Instance.SendJobRequestToManager(new JobForWorker(vector2Int, JobType.Transport, this));
                Debug.Log($"{Name} đã sản xuất {productionAmount} sản phẩm!");
        }
        
        OnProductionCompleted();
    }

    private bool ConsumeInputs()
    {
        foreach (var item in inputItems)
        {
          int amount =  Singleton<ItemStorage>.Instance.TryTakeItem(item.ID, 10);
            if(amount <= 0)
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

    protected virtual void OnProductionCompleted()
    {
        Debug.Log($"{Name} đã sản xuất xong!");
    }

    public void StartProduction()
    {
        if (!isProducing && !IsDestroyed)
        {
            currentProductionTime = 0f;
        }
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

    public void Produce()
    {
        if (isProducing)
        {
            CompleteProduction();
        }
    }


    public override void OnBuildingDestroyed()
    {
        base.OnBuildingDestroyed();
        StopProduction();
        
        // Rơi vật phẩm khi công trình bị phá hủy
        if (itemDropper != null)
        {
            itemDropper.DropFunction(transform.position, true);
        }
    }

    public override void UpdateBuilding()
    {
        // Cập nhật trạng thái công trình
    }
















    
}
