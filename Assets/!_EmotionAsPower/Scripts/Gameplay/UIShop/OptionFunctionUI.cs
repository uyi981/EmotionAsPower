using System.Collections;
using UnityEngine;
using Assets.__EmotionAsPower.Scripts.Gameplay.Building.BuidingChild;
using System.Xml.Serialization;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.UIShop
{
    public class OptionFunctionUI : MonoBehaviour
    {
        [Tooltip("Reference to the BreedingBuilding component")]

        [SerializeField] private BreedingBuilding breedingBuilding;
        [SerializeField] private BuildingBase buildingBase;


        private void Start()
        {
            // Tìm tất cả các component BreedingBuilding trong các đối tượng con
            breedingBuilding = GetComponentInParent<BreedingBuilding>(true);
            buildingBase = GetComponentInParent<BuildingBase>(true);

        }

        /// <summary>
        /// Gọi khi nhấn nút UI để kích hoạt quá trình sinh sản
        /// </summary>
        public void BreedButtonClicked()
        {
            if (breedingBuilding == null)
            {
                Debug.LogError("Không có BreedingBuilding nào được tìm thấy!");
                return;
            }


                breedingBuilding.Breed();
                Debug.Log("Đã kích hoạt chức năng sinh sản!");
        }

        public void RepairBuilding()
        {
            if (buildingBase == null)
            {
                Debug.LogError("Không có BuildingBase nào được tìm thấy!");
                return;
            }
                buildingBase.RepairBuilding(5);
                Debug.Log("Đã kích hoạt chức năng sửa chữa công trình!");
        }

        public void DestroyBuilding()
        {
            if (buildingBase == null)
            {
                Debug.LogError("Không có BuildingBase nào được tìm thấy!");
                return;
            }
            buildingBase.OnBuildingDestroyed();
            Debug.Log("Đã kích hoạt chức năng phá hủy công trình!");
        }

        public void MoveBuilding()
        {
            if (buildingBase == null)
            {
                Debug.LogError("Không có BuildingBase nào được tìm thấy!");
                return;
            }
            buildingBase.MoveBuilding();
            Debug.Log("Đã kích hoạt chức năng di chuyển công trình!");
        }
    }
}