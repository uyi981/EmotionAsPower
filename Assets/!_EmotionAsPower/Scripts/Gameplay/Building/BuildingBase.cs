using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Assets.__EmotionAsPower.Scripts.UI.ProcessBar;
using LgTyUtils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BuildingBase : MonoBehaviour, IBuilding, IInteractable
{
    [Header("Building Properties")]
    [Tooltip("Công trình đang được chọn để xây dựng")]
    public Building selectedBuilding;
    [Tooltip("Tiến độ xây dựng công trình từ 0-1")]
    [Range(0, 1)]
    public float buildProgress = 0f;
    [Tooltip("Thời gian xây dựng công trình (giây)")]
    public float buildTime;


    [Tooltip("Công trình đã xây xong")]
    public bool isBuild = false;
    [Tooltip("Số lượng công nhân tham gia xây dựng")]
    public int workersAmount = 0;
    
    [Header("Building Stats")]
    [Tooltip("Máu tối đa của công trình")]
    public int maxHP = 100;
    [Tooltip("Máu hiện tại của công trình")]
    public int currentHP;
    [Tooltip("Danh sách các vật phẩm cần thiết để xây dựng công trình")]
    public SerializableDictionary<ItemSO, int> requiredItems = new SerializableDictionary<ItemSO, int>();



    [Header("Postion Task of Worker")]
    [Tooltip("Vị trí của công nhân trong quá trình xây dựng")]
    public List<Vector2Int> workerPositions;
    [Tooltip("Loại công việc của công trình")]
    public JobType jobType;

    [Header("Item Drop")]
    [Tooltip("Thành phần xử lý rơi vật phẩm khi công trình bị phá hủy")]
    [SerializeField] protected ItemDropper itemDropper;

    [Header("Bar UI")]
    [Tooltip("Thanh tiến độ")]
    public GameObject buildingBar;
    [Tooltip("Thanh HP")]
    public GameObject healthBar;



    public Vector3Int baseCell;
    private int selectedBuildingID;
    private ProcessBar processBarImg;
    private HealthBar healthBarImg;
    private GameObject processBarInstance;
    private GameObject healthBarInstance;
    private bool isBuildingComplete = false;
    private bool isDestroyed = false;
    protected List<Villager> workers = new List<Villager>();

    public int ID { get => selectedBuildingID; set => selectedBuildingID = value; }
    public string Name => gameObject.name;

    public int Health => currentHP;

    public int MaxHealth => maxHP;

    public bool IsDestroyed => isDestroyed;

    public bool IsBuild => isBuild;

    public JobType JobType { get; set; }

    public Vector3Int BaseCell
    {
        get => baseCell;
        set => baseCell = value;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(30); // Giả lập việc công trình bị tấn công
        }
    }

    public virtual void Start()
    {

        workersAmount = 0; // Khởi tạo số lượng công nhân tham gia xây dựng    
        currentHP = maxHP; // Khởi tạo máu hiện tại bằng máu tối đa
        buildTime = selectedBuilding.buildTime; // Lấy thời gian xây dựng từ SO_Building
        Debug.Log($"Building {Name} started with build time: {buildTime} seconds");
        AssignJobToWorker(JobType.Build);
        // Tìm Image con tên 'Fill' trong processBarInstance và healthBarInstance
        if (buildingBar != null)
        {
            processBarInstance = Instantiate(buildingBar, transform.position + Vector3.up * 3f, Quaternion.identity, transform);
            processBarImg = processBarInstance.GetComponent<ProcessBar>();
            processBarImg.SetProcess(0f); // Đặt tiến độ ban đầu là 0
        }
        if (healthBar != null)
        {
            healthBarInstance = Instantiate(healthBar, transform.position + Vector3.up * 3f, Quaternion.identity, transform);
            healthBarInstance.SetActive(false); // Ẩn thanh máu ban đầu
            healthBarImg = healthBarInstance.GetComponent<HealthBar>(); 
        }
        Singleton<DayTimeController>.Instance.OnTimeStageChanged += OnDayStageChange; 
        StartCoroutine(Building()); // Bắt đầu quá trình xây dựng

    }



    /// <summary>
    /// Phương thức này sẽ tiêu thụ các vật phẩm cần thiết để xây dựng công trình.
    /// </summary>
    /// <returns></returns>
    public bool TryConsumeRequiredItems()
    {
      

        foreach (var pair in requiredItems)
        {
            ItemSO item = pair.Key;
            int amount = pair.Value;
            int checkAmount = Singleton<ItemStorage>.Instance.TryTakeItem(item, amount);
            if(checkAmount <= 0)
            {
                Debug.LogWarning($"Not enough {item.ID} to consume for {Name}. Required: {amount}, Available: {checkAmount}");
                return false; // Không đủ vật phẩm cần thiết
            }
            Debug.Log($"Consumed {amount} of {item.ID} for {Name}");
        }

        Debug.Log($"Consumed all required items for {Name}");
        return true;
    }
    public void OnDayStageChange(DayTimeController.TimeStage timeStage)
    {
        if (timeStage == DayTimeController.TimeStage.Morning)
        {
            AssignJobToWorker(jobType);
        }
    }

    /// <summary>
    /// Coroutine để xây dựng công trình.
    /// </summary>
    /// <returns></returns>
    public IEnumerator Building()
    {
        float time = buildTime;
        while (time > 0f)
        {
            yield return new WaitForSeconds(0.05f);
            time -= 0.1f*workersAmount;
            buildProgress = time / buildTime;
            processBarImg.SetProcess(1-buildProgress);
        }
        buildProgress = 1f; // Hoàn thành xây dựng
        isBuild = true;
        processBarInstance.SetActive(false);
        healthBarInstance.SetActive(true); // Hiển thị thanh máu khi công trình đã hoàn thành
        workersAmount = 0;
        OnBuildingComplete();
        isBuildingComplete = true; // Đánh dấu công trình đã hoàn thành xây dựng
    }

    

    /// <summary>
    /// Phương thức này được gọi để reset danh sách công nhân.
    /// </summary>
    public void ResetWorkerList()
    {
        foreach (Villager worker in workers)
        {
            worker.TransitionTo(worker.villagerIdleState); // Trả công nhân về trạng thái nhàn rỗi
        }
        workers.Clear();
    }
   
    private void UpdateHealthBar()
    {
        if (healthBarImg != null)
        {
            healthBarImg.SetHealth(currentHP);
        }
    }

    

    /// <summary>
    /// Phương thức này được gọi khi một công nhân đến làm việc tại công trình.
    /// Tăng số lượng công nhân và thêm công nhân vào danh sách nếu chưa có.
    /// </summary>
    /// <param name="villager"></param>
    public void OnWorkerCome(Villager villager)
    {
        workersAmount++;
        if(workers.Contains(villager))
        {
            return;
        }
        workers.Add(villager);
    }    

    ////////////////////////////
    ///// Viet phuong thuc giao việc cho công nhân
    public void AssignJobToWorker(JobType jobType)
    {
        this.jobType = jobType;
        Debug.Log($"Assigning job of type {jobType} to workers at positions: {workerPositions.Count}");
        
        if (workerPositions == null || workerPositions.Count == 0)
        {
            Debug.LogWarning($"No worker positions defined for {gameObject.name}");
            return;
        }

        foreach (Vector2Int position in workerPositions)
        {
            // Create job for each worker position
            JobForWorker job = new JobForWorker(position, jobType, this);
            
            // Send job request to VillagerManager
            if (VillagerManager.Instance != null)
            {
                Singleton<VillagerManager>.Instance.SendJobRequestToManager(job);
            }
            else
            {
                Debug.LogError("VillagerManager instance is not available!");
                return;
            }
        }

        // Try to assign all available jobs to villagers
        if (VillagerManager.Instance != null)
        {
            VillagerManager.Instance.SendJobToVillager();
        }

        Debug.Log($"Assigned jobs for {workerPositions.Count} worker positions for building {gameObject.name}");
    }

    public void OnInteract()
    {
        //throw new System.NotImplementedException();
    }

    public InteractableType GetInteractableType() => InteractableType.Building;






    // Interface methods for IBuilding
    ////////////////////////////////////
    public virtual void TakeDamage(int damage)
    {
        if (!isBuildingComplete) return;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        UpdateHealthBar();

        //if (currentHP <= 0)
        //{
        //    OnBuildingDestroyed();
        //}
    }
    public virtual void RepairBuilding(int amount)
    {
        StartCoroutine(Repair(amount));
    }

    /// <summary>
    /// Coroutine để sửa chữa công trình.
    /// </summary>
    private IEnumerator Repair(int amount)
    {
        Debug.Log($"Starting repair for {gameObject.name} with amount: {amount}");
        while (currentHP < maxHP)
        {
            yield return new WaitForSeconds(1f); // Kiểm tra mỗi 0.5 giây

            // Tăng máu dựa trên số lượng công nhân đang sửa chữa
            int healAmount = workersAmount * amount; // Mỗi công nhân hồi 5 máu
            currentHP = Mathf.Min(currentHP + healAmount, maxHP);
            UpdateHealthBar();
            Debug.Log($"Repairing {gameObject.name}: Current HP = {currentHP}/{maxHP}");
            // Nếu đã hồi đủ máu thì dừng
            if (currentHP >= maxHP)
            {
                currentHP = maxHP;
            }
        }
        ResetWorkerList();

    }

    public virtual void MoveBuilding()
    {
        PlacementSystem placementSystem = FindFirstObjectByType<PlacementSystem>();
        placementSystem.OccupyCells(baseCell, 1);
        placementSystem.StartPlacement(selectedBuildingID);
        AssignJobToWorker(JobType.Build);
        Destroy(gameObject); // Xóa công trình hiện tại
    }

    /// <summary>
    /// Phương thức này được gọi khi công trình bị phá hủy.
    /// </summary>
    public virtual void OnBuildingDestroyed()
    {
    
        
        // Spawn half of each required item if there are any
        if (requiredItems != null && requiredItems.Count > 0)
        {
            foreach (var item in requiredItems)
            {
                int halfAmount = Mathf.Max(1, item.Value / 2); // At least 1 item
                ItemManager.Instance.SpawnItem(item.Key, halfAmount, transform.position + new Vector3(0, 0.5f, -(1 + selectedBuilding.size.y)));
                Debug.Log($"Spawned {halfAmount} of {item.Key.ID} at position {transform.position + new Vector3(0, 0.5f, -(1 + selectedBuilding.size.y))}");
            }
        }
        
        isDestroyed = true; // Đánh dấu công trình đã bị phá hủy
        Destroy(gameObject);
        ResetWorkerList();
    }

    /// <summary>
    /// Phương thức này được gọi khi công trình hoàn thành xây dựng.
    /// </summary>
    public virtual void OnBuildingComplete()
    {
        ResetWorkerList(); // Reset danh sách công nhân
    }
    //////////////////////////////////////
    ///


}
