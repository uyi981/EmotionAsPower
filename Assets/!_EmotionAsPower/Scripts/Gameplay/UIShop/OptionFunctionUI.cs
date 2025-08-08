using System.Collections;
using UnityEngine;
using Assets.__EmotionAsPower.Scripts.Gameplay.Building.BuidingChild;
using System.Xml.Serialization;
using TMPro;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.UIShop
{
    public class OptionFunctionUI : MonoBehaviour
    {
        [Tooltip("Reference to the BreedingBuilding component")]

        [SerializeField] private BreedingBuilding breedingBuilding;
        [SerializeField] public BuildingBase buildingBase;
        BuildingBase buildingA;
        public GameObject BreedingButton;
        public TextMeshProUGUI hp;
        public void UpdateUI(BuildingBase building)
        {
            if(building == null)
            {
                Debug.LogError("BuildingBase is null!");
                return;
            }
            buildingBase = building;
            buildingA = building;
            Debug.Log("Updating UI for building: " + buildingBase.gameObject.name);
            if (buildingBase.buildingType.Equals(BuildingType.Breeding))
            {
                BreedingButton.SetActive(true);
                breedingBuilding = buildingBase as BreedingBuilding;
            }
            else
            {
                BreedingButton.SetActive(false);
            }
            SetInfo(buildingBase.Health, buildingBase.MaxHealth);
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


                breedingBuilding.OpenBreedingUI();
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
            Singleton<DetailInfoController>.Instance.CloseUI();
        }

        public void MoveBuilding()
        {
            //if (buildingBase == null)
            //{
            //    Debug.LogError("Không có BuildingBase nào được tìm thấy!");
            //    return;
            //}
            Debug.Log("buildingBase" + buildingBase.gameObject.name);
            buildingBase.MoveBuilding();
            Debug.Log("Đã kích hoạt chức năng di chuyển công trình!");
        }

        public void SetInfo(int currentHP, int maxHP)
        {
            if (hp == null)
            {
                Debug.LogError("Không có TextMeshProUGUI hp nào được tìm thấy!");
                return;
            }
            hp.text = "HP: " + currentHP + "/" + maxHP;
        }
    }
}