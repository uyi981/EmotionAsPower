using System.Collections;
using System.ComponentModel;
using Assets.__EmotionAsPower.Scripts.UI.ProcessBar;
using UnityEngine;
using UnityEngine.UI;

public class BuildingBase : MonoBehaviour, IBuilding
{
    [Header("Cấu hình Building")]
    [Tooltip("Tiến độ xây dựng công trình từ 0-1")]
    [Range(0, 1)]
    public float buildProgress = 0f;
    [Tooltip("Thời gian xây dựng công trình (giây)")]
    public float buildTime;
    [Tooltip("Công trình đang được chọn để xây dựng")]
    public Building selectedBuilding;
    [Tooltip("Công trình đã xây xong")]
    public bool isBuild = false;
    [Tooltip("Thời gian đã trôi qua kể từ khi bắt đầu xây dựng")]
    public float time;
    [Tooltip("Số lượng công nhân tham gia xây dựng")]
    public int workersAmount = 0;
    [Tooltip("Thanh tiến độ")]
    public GameObject processBar;
    [Tooltip("Thanh HP")]
    public GameObject healthBar;
    [Tooltip("Máu tối đa của công trình")]
    public int maxHP = 100;
    [Tooltip("Máu hiện tại của công trình")]
    public int currentHP;


    private ProcessBar processBarImg;
    private HealthBar healthBarImg;
    private GameObject processBarInstance;
    private GameObject healthBarInstance;
    private bool isBuildingComplete = false;
    private bool isDestroyed = false;

    public string Name => gameObject.name;

    public int Health => currentHP;

    public int MaxHealth => maxHP;

    public bool IsDestroyed => isDestroyed;

    public bool IsBuild => isBuild;

    public virtual void Start()
    {
        currentHP = maxHP; // Khởi tạo máu hiện tại bằng máu tối đa
        buildTime = selectedBuilding.buildTime; // Lấy thời gian xây dựng từ SO_Building

        // Tìm Image con tên 'Fill' trong processBarInstance và healthBarInstance
        if (processBar != null)
        {
            processBarInstance = Instantiate(processBar, transform.position + Vector3.up * 3f, Quaternion.identity, transform);
            processBarImg = GetComponentInChildren<ProcessBar>();
            processBarImg.SetProcess(0f); // Đặt tiến độ ban đầu là 0
        }
        if (healthBar != null)
        {
            healthBarInstance = Instantiate(healthBar, transform.position + Vector3.up * 3f, Quaternion.identity, transform);
            healthBarInstance.SetActive(false); // Ẩn thanh máu ban đầu
            
        }
    }

    private void OnEnable()
    {
        
        //isBuild = true;
    }

    public virtual void Update()
    {


        if (!isBuild && workersAmount > 0)
        {
            IsBuildingComplete();
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage(10); // Giả lập việc công trình bị tấn công
        }
    }

    /// <summary>
    /// Kiểm tra xem công trình đã hoàn thành xây dựng hay chưa.
    /// </summary>
    public void IsBuildingComplete()
    {
        time += Time.deltaTime * workersAmount;
        // Cập nhật tiến độ xây dựng (giả sử mỗi giây tăng 0.2, bạn có thể thay đổi logic này)
        if (time > buildTime)
        {
            buildProgress = 1f; // Hoàn thành xây dựng
            isBuild = true;
            processBarInstance.SetActive(false);
            healthBarInstance.SetActive(true);
            healthBarImg = GetComponentInChildren<HealthBar>();
            isBuildingComplete = true; // Đánh dấu công trình đã hoàn thành xây dựng
        }
        else
        {
            buildProgress = time / buildTime; // Tính toán tiến độ từ 0 đến 1
        }
        processBarImg.SetProcess(buildProgress); // Cập nhật thanh tiến độ UI
    }


    





    /// <summary>
    /// Cập nhật thanh máu của công trình dựa trên máu hiện tại và tối đa.
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthBarImg != null)
        {
            healthBarImg.SetHealth((float)currentHP / maxHP);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (!isBuildingComplete) return;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        UpdateHealthBar();

        if (currentHP <= 0)
        {
            // Handle building destroyed logic here
            Destroy(gameObject);
        }
    }

    public virtual void Heal(int amount)
    {
        
    }

    public virtual void UpdateBuilding()
    {
       
    }

    public virtual void OnBuildingDestroyed()
    {
       
    }
}
