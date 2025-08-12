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
    public Queue<Vector2Int> empltyWokerSlot = new Queue<Vector2Int>();


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
    public override void OnWorkerLeave(Villager villager)
    {
        Debug.Log($"Worker {villager.name} has left the building {Name}.");
        workersAmount--;
        if (workers.Contains(villager))
        {
            workers.Remove(villager);
            empltyWokerSlot.Enqueue(villager.currentJob.Position);
        }

    }


    public bool CheckIsHaveEmptyJob(Villager villager)
    {
        if (empltyWokerSlot.Count > 0)
        {
            if (villager.isWorking == false)
            {
                villager.currentJob = new JobForWorker(empltyWokerSlot.Dequeue(), JobType.Produce, this);
                Debug.Log("Assigning job to villager: " + villager.name + " with job type: " + villager.currentJob.JobType);
                villager.TransitionTo(villager.villagerWorkingState);
            }
            return true;
        }
        return false;
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
    public float GetWorkerSpeedModifier()
    {
        float speedModifier = 0f;
        foreach (var worker in workers)
        {
            if (worker != null && worker.personality != null)
            {
                speedModifier += worker.personality.worKSpeedModifier;
            }
        }
        return speedModifier;
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
                currentProductionTime += 0.1f*GetWorkerSpeedModifier();
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
        StartCoroutine(Wait());
    }
    public IEnumerator Wait()
    {
        yield return new WaitForSeconds(2f);
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
            SpawnFoodFactoryOutputItems();
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


    public void SpawnFoodFactoryOutputItems()
    {
        if (selectedBuilding != null && selectedBuilding.type == BuildingType.FoodFactory)
        {
            // Calculate base positions
            Vector3 basePosition = transform.position + new Vector3(0, 0.5f, 0);

            // Directions relative to building's forward
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // Spawn positions: in front, left, right
            Vector3[] spawnPositions = new Vector3[3];
            spawnPositions[0] = basePosition + forward * (1 + selectedBuilding.size.y);      // In front
            spawnPositions[1] = basePosition - right * (0.5f * selectedBuilding.size.x);      // Left
            spawnPositions[2] = basePosition + right * (0.5f * selectedBuilding.size.x);      // Right

            for (int i = 0; i < outputItems.Count && i < 3; i++)
            {
                ItemManager.Instance.SpawnItem(outputItems[i], productionAmount, spawnPositions[i]);
            }
            Debug.Log($"{Name} (FoodFactory) đã spawn {Mathf.Min(outputItems.Count, 3)} sản phẩm tại 3 vị trí!");
        }
        else
        {
            ItemManager.Instance.SpawnItem(outputItems[0], productionAmount, transform.position + new Vector3(0, 0.5f, -(1 + selectedBuilding.size.y)));

        }
    }




}
