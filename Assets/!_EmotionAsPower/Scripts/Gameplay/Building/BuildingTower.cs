using System.Collections;
using UnityEngine;
using UnityEngine.UI;

    public class BuildingTower : MonoBehaviour
    {
        [Header("Cấu hình Building")]
        [Tooltip("Tiến độ xây dựng công trình từ 0-1")]
        [Range(0, 1)]
        public float buildProgress = 0f;
        [Tooltip("Thời gian xây dựng công trình (giây)")]
        public float buildTime;
        [Tooltip("Công trình đang được chọn để xây dựng")]
        public Building selectedBuilding;
        [Tooltip("Công trình đang xây dựng")]
        public bool isBuilding = false;
        [Tooltip("Thời gian đã trôi qua kể từ khi bắt đầu xây dựng")]
        public float time;
        [Tooltip("Số lượng công nhân tham gia xây dựng")]
        public int workersAmount = 0;
        [Tooltip("Thành phần UI hiển thị tiến độ xây dựng")]
        public Image processBar;
        [Tooltip("Thành phần UI hiển thị thanh máu của công trình")]
        public Image healthBar;


        private void Start()
        {
            buildTime = selectedBuilding.buildTime; // Lấy thời gian xây dựng từ SO_Building
        }

        private void OnEnable()
        {
            isBuilding = true;
        }

        void Update()
        {
         

            if(isBuilding)
            {
                IsBuildingComplete();
            }
        }


        public void IsBuildingComplete()
        {
             time += Time.deltaTime * workersAmount;
            // Cập nhật tiến độ xây dựng (giả sử mỗi giây tăng 0.2, bạn có thể thay đổi logic này)
            if (time > buildTime)
            {
                buildProgress = 1f; // Hoàn thành xây dựng
                isBuilding = false;
            }
            else
            {
                buildProgress = time / buildTime; // Tính toán tiến độ từ 0 đến 1
            }
            processBar.fillAmount = buildProgress; // Cập nhật thanh tiến độ UI
        }
    }
