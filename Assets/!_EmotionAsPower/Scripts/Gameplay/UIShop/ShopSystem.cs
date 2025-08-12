using System.Collections.Generic;
using Assets.__EmotionAsPower.Scripts.Gameplay.Building.BuidingChild;
using LgTyUtils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSystem : Singleton<ShopSystem>
{
    [Header("References")]
    public ObjectsDatabaseSO buildingObjects;
    public GameObject tabOption;
    public GameObject contentManager;

    [Header("Prefabs")]
    public GameObject buldingContentUI;
    public Information info;
    public GameObject tab;
    public Sprite gridFrame;

    // Dictionary lưu ContentUI và danh sách building theo loại
    private Dictionary<BuildingType, ContentUI> contentUIs = new Dictionary<BuildingType, ContentUI>();
    private Dictionary<BuildingType, List<GameObject>> buildingLists = new Dictionary<BuildingType, List<GameObject>>();
    private Image currentImage;

    public void SetGridFrame(Image isBuildingSelected)
    {
        if (currentImage != null)
        {
            currentImage.enabled = false;
        }
        isBuildingSelected.enabled = true;
        currentImage = isBuildingSelected;


    }

    public void ResetGridFrame()
    {
        if (currentImage != null)
        {
            currentImage.enabled = false;
        }
    }
    private void Start()
    {
        CategorizeBuildings();
    }

    private void CategorizeBuildings()
    {
        contentUIs.Clear();
        buildingLists.Clear();

        // Duyệt qua tất cả building và nhóm theo type
        foreach (var building in buildingObjects.buildings)
        {
            if (building.buildingPrefab == null) continue;

            // Nếu type chưa có content UI -> tạo mới
            if (!contentUIs.ContainsKey(building.type))
            {
                // Tạo ContentUI
                var contentObj = Instantiate(buldingContentUI, contentManager.transform);
                var contentUI = contentObj.GetComponent<ContentUI>();
                contentUIs[building.type] = contentUI;

                // Khởi tạo list buildings
                buildingLists[building.type] = new List<GameObject>();

                // Tạo Tab
                CreateTab(building.type, building.type.ToString(), contentObj);
            }

            // Gán thông tin building vào ContentUI tương ứng
            var targetContentUI = contentUIs[building.type];
            targetContentUI.textMeshPro.text = building.type.ToString();
            targetContentUI.InstantiateIcon();
            targetContentUI.SetImage(building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
            targetContentUI.SetResource(building.keyValuePairs);
            targetContentUI.SetInfo(building.buildingID,
                building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite,
                building.buildingName, building.description);

            // Thêm prefab vào list
            buildingLists[building.type].Add(building.buildingPrefab);
        }

        Debug.Log($"Categorized buildings: {buildingObjects.buildings.Count} total");
    }

    private void CreateTab(BuildingType type, string tabName, GameObject contentObject)
    {
        GameObject newTab = Instantiate(tab, tabOption.transform);

        // Set tab text
        var tabText = newTab.GetComponentInChildren<TextMeshProUGUI>();
        if (tabText != null) tabText.text = tabName;

        // Gắn sự kiện click
        var tabButton = newTab.GetComponent<Button>();
        if (tabButton == null) tabButton = newTab.AddComponent<Button>();

        tabButton.onClick.AddListener(() => OnTabClicked(type));

        contentObject.SetActive(false);
    }

    private void OnTabClicked(BuildingType type)
    {
        // Ẩn tất cả content
        foreach (var content in contentUIs.Values)
            content.gameObject.SetActive(false);

        // Hiện content được chọn
        if (contentUIs.ContainsKey(type))
            contentUIs[type].gameObject.SetActive(true);
        info.ResetInformationData();
        ResetGridFrame();
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
    }
}
