using System.Collections;
using UnityEngine;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.UIShop
{
    public class OptionUI : MonoBehaviour
    {
        [Tooltip("Công trình có được chọn hay không")]
        public bool isSelected = false;
        [Tooltip("UI công trình đang xây dựng")]
        public GameObject buildingUI;
        [Tooltip("Vị trí hiển thị UI công trình")]
        public Vector3 UIPosition;

        private BuildingBase buildingComponents;
        private GameObject buildingUIInstance;

        private void Start()
        {
            buildingComponents = gameObject.GetComponentInChildren<BuildingBase>(true);
            buildingUIInstance = Instantiate(buildingUI, transform.position + UIPosition, buildingUI.transform.rotation, transform);
            buildingUIInstance.SetActive(false); // Ẩn UI ban đầu
        }

        private void OnMouseDown()
        {
            // Kiểm tra xem có building component nào không
            if (buildingComponents == null)
            {
                Debug.LogWarning("Không tìm thấy BuildingBase component");
                return;
            }

            isSelected = !isSelected; // Đảo trạng thái chọn công trình
            buildingUIInstance.SetActive(isSelected); // Hiển thị hoặc ẩn UI công trình
            return;


            Debug.Log("Không thể chọn công trình chưa xây xong");
        }
    }
}