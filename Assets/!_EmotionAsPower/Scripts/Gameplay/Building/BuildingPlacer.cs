using System.ComponentModel;
using Assets.__EmotionAsPower.Scripts.Gameplay.Building;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    [Header("Cấu hình Building Placer")]
    [Tooltip("Camera chính để theo dõi vị trí chuột")]
    public Camera mainCamera;
    [Tooltip("Layer mask để xác định các bề mặt có thể đặt công trình")]
    public LayerMask placementLayer;
    [Tooltip("Công trình đang được chọn để xây dựng")]
    public SO_Building selectedBuilding;


    [ReadOnly(true)] 
    public GameObject blueprintInstance;
 


  

    void Update()
    {
        if (selectedBuilding != null)
        {
            HandleBlueprintFollowMouse();
            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceBuilding();
            }
        }
    }

    void HandleBlueprintFollowMouse()
    {
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Vẽ tia raycast màu xanh lá, dài 100 đơn vị, hiện trong Scene view
        Debug.DrawRay(ray.origin, ray.direction * 200f, Color.green);

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, placementLayer))
        {
            if (blueprintInstance == null)
            {
                blueprintInstance = Instantiate(selectedBuilding.blueprintPrefab);
            }
            blueprintInstance.transform.position = hit.point;
        }
    }

    void TryPlaceBuilding()
    {
        // Instantiate công trình thật
        GameObject buildingObj = Instantiate(selectedBuilding.buildingPrefab, blueprintInstance.transform.position, Quaternion.identity);

        // Truyền SO_Building cho script Building trên prefab
        Building buildingScript = buildingObj.GetComponent<Building>();
        if (buildingScript != null)
        {
            buildingScript.selectedBuilding = this;
        }

        Destroy(blueprintInstance);
        //selectedBuilding = null;

    }

    // Kiem tra xem xay dung cong trinh hoan thanh chua

    // Hàm này gọi khi chọn công trình từ Shop
    public void SelectBuilding(SO_Building building)
    {
        selectedBuilding = building;
        if (blueprintInstance != null)
            Destroy(blueprintInstance);
    }
}
