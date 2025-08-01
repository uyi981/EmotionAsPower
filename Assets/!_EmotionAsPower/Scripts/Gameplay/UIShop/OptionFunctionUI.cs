using System.Collections;
using UnityEngine;
using Assets.__EmotionAsPower.Scripts.Gameplay.Building.BuidingChild;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.UIShop
{
    public class OptionFunctionUI : MonoBehaviour
    {
        [Tooltip("Reference to the BreedingBuilding component")]

        [SerializeField]  private BreedingBuilding[] breedingBuildings;

        private void Start()
        {
            // Tìm tất cả các component BreedingBuilding trong các đối tượng con
            breedingBuildings = GetComponentsInParent<BreedingBuilding>(true);
            
            if (breedingBuildings.Length == 0)
            {
                Debug.LogWarning("Không tìm thấy BreedingBuilding nào trong các đối tượng con!");
            }
        }

        /// <summary>
        /// Gọi khi nhấn nút UI để kích hoạt quá trình sinh sản
        /// </summary>
        public void OnBreedButtonClicked()
        {
            if (breedingBuildings == null || breedingBuildings.Length == 0)
            {
                Debug.LogError("Không có BreedingBuilding nào được tìm thấy!");
                return;
            }

            foreach (var breedingBuilding in breedingBuildings)
            {
                if (breedingBuilding != null)
                {
                    breedingBuilding.Breed();
                    Debug.Log("Đã kích hoạt chức năng sinh sản!");
                }
            }
        }
    }
}