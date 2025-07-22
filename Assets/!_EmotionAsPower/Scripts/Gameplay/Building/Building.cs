using System.Collections;
using UnityEngine;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.Building
{
    public class Building : MonoBehaviour
    {
        [Tooltip("Tiến độ xây dựng công trình từ 0-1")]
        [Range(0, 1)]
        public float buildProgress = 0f;
        [Tooltip("Thời gian xây dựng công trình (giây)")]
        public float buildTime;
        [Tooltip("Công trình đang được chọn để xây dựng")]
        public BuildingPlacer selectedBuilding;
        [Tooltip("Công trình đang xây dựng")]
        public bool isBuilding = false;
        [Tooltip("Thời gian đã trôi qua kể từ khi bắt đầu xây dựng")]
        public float time;

        private void Start()
        {
            buildTime = selectedBuilding.selectedBuilding.buildTime; // Lấy thời gian xây dựng từ SO_Building
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


        public bool IsBuildingComplete()
        {
             time += Time.deltaTime;
            // Cập nhật tiến độ xây dựng (giả sử mỗi giây tăng 0.2, bạn có thể thay đổi logic này)
            if (time > buildTime)
            {
                buildProgress = 1f; // Hoàn thành xây dựng
                isBuilding = false;
                return true;
            }
            else
            {
                buildProgress = time / buildTime; // Tính toán tiến độ từ 0 đến 1
                return false; // Chưa hoàn thành
            }
        }
    }
}