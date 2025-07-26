using UnityEngine;
using System.Collections.Generic;

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
        StartProduction();
    }

    public override void Update()
    {
        base.Update();
        if(IsBuild) isProducing = true; 
        if (isProducing) UpdateProduction();
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

    private bool HasRequiredInputs()
    {
        // Kiểm tra xem có đủ nguyên liệu đầu vào không
        // Cần implement InventorySystem để kiểm tra
        return true; // Tạm thời luôn trả về true
    }

    private void CompleteProduction()
    {
        // Tiêu thụ nguyên liệu đầu vào
        ConsumeInputs();
        
        // Tạo sản phẩm
        if (dropItemsOnProduction && itemDropper != null)
        {

            // Dùng ItemManager.Instance.SpawnItem spawn mang outputItems
            for (int i = 0; i < outputItems.Count; i++)
            {
                ItemManager.Instance.SpawnItem(outputItems[i], productionAmount, transform.position);
            }
            Debug.Log($"{Name} đã sản xuất {productionAmount} sản phẩm!");
        }
        
        OnProductionCompleted();
    }

    private void ConsumeInputs()
    {
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
