using System.Collections;
using System.Collections.Generic;
using Assets.__EmotionAsPower.Scripts.Gameplay.Building.BuidingChild;
using Assets.__EmotionAsPower.Scripts.Gameplay.UIShop.Content;
using LgTyUtils;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.UIShop
{
    public class ShopSystem : MonoBehaviour
    {
        [Header("Building Lists")]
        public List<GameObject> listProductionBuildings = new List<GameObject>();
        public List<GameObject> listBedBuildings = new List<GameObject>();
        public List<GameObject> listTowerBuildings = new List<GameObject>();
        public List<GameObject> listDecorationBuildings = new List<GameObject>();
        public List<GameObject> listDefenseBuildings = new List<GameObject>();
        public List<GameObject> listMainBaseBuildings = new List<GameObject>();

        [Header("References")]
        public ObjectsDatabaseSO buildingObjects;


        [Header("Content Manager")]
        public GameObject contentManager;


        [Header("UI Settings")]
        //public GameObject iconUI; // Reference to the shop UI prefab
        public GameObject buldingContentUI; // Reference to the content UI prefab

        [Header("Info Panel")]
        public GameObject info;

        GameObject productionBulding;
        GameObject towerBuilding;
        GameObject bedBuilding;
        GameObject decorationBuilding;
        GameObject defenseBuilding;
        GameObject infoInstance;


        private void Start()
        {
            // Instantiate the shop content UI prefab
            productionBulding = Instantiate(buldingContentUI, contentManager.transform);
            towerBuilding = Instantiate(buldingContentUI, contentManager.transform);
             bedBuilding = Instantiate(buldingContentUI, contentManager.transform);
           decorationBuilding = Instantiate(buldingContentUI, contentManager.transform);
            defenseBuilding = Instantiate(buldingContentUI, contentManager.transform);

            infoInstance = Instantiate(info, transform);
            infoInstance.SetActive(false); 



            CategorizeBuildings();
        }

        private void CategorizeBuildings()
        {
            // Clear all lists
            listProductionBuildings.Clear();
            listBedBuildings.Clear();
            listTowerBuildings.Clear();
            listDecorationBuildings.Clear();
            listDefenseBuildings.Clear();
            listMainBaseBuildings.Clear();

            // Categorize each building from the ScriptableObject
            foreach (var building in buildingObjects.buildings)
            {
                if (building.buildingPrefab == null) continue;


                // Get the ContentUI component from the instantiated prefab
                ContentUI contentUIInstance = productionBulding.GetComponent<ContentUI>();
                ContentUI contentUITowerInstance = towerBuilding.GetComponent<ContentUI>();
                ContentUI contentUIBedInstance = bedBuilding.GetComponent<ContentUI>();
                ContentUI contentUIDecorationInstance = decorationBuilding.GetComponent<ContentUI>();
                ContentUI contentUIDefenseInstance = defenseBuilding.GetComponent<ContentUI>();
                // Set the TextMeshProUGUI text based on building type
                if (contentUIInstance != null && contentUIInstance.textMeshPro != null)
                {
                    switch (building.type)
                    {
                        case BuildingType.Production:
                            contentUIInstance.textMeshPro.text = "Production";
                            contentUIInstance.InstantiateIcon();
                            contentUIInstance.SetImage(building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                            contentUIInstance.SetResource(building.keyValuePairs);
                            contentUIInstance.SetInformation(infoInstance, building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite,
                                building.buildingName, building.description);
                            Debug.Log($"Adding production buildinggggggggggggggggggggggg: " + building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite.name);
                            listProductionBuildings.Add(building.buildingPrefab);
                            break;
                        case BuildingType.Bed:
                            contentUIBedInstance.textMeshPro.text = "Bed";
                            contentUIBedInstance.InstantiateIcon();
                            contentUIBedInstance.SetImage(building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                            contentUIBedInstance.SetResource(building.keyValuePairs);
                            contentUIBedInstance.SetInformation(infoInstance, building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite,
                                building.buildingName, building.description);
                            listBedBuildings.Add(building.buildingPrefab);
                            break;
                        case BuildingType.Tower:
                            contentUITowerInstance.textMeshPro.text = "Tower"; contentUITowerInstance.InstantiateIcon();
                            contentUITowerInstance.SetImage(building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                            contentUITowerInstance.SetResource(building.keyValuePairs);
                            contentUITowerInstance.SetInformation(infoInstance, building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite,
                                building.buildingName, building.description);
                            listTowerBuildings.Add(building.buildingPrefab);
                            break;
                        case BuildingType.Decoration:
                            contentUIDecorationInstance.textMeshPro.text = "Decoration"; contentUIDecorationInstance.InstantiateIcon();
                            contentUIDecorationInstance.SetImage(building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                            contentUIDecorationInstance.SetResource(building.keyValuePairs);

                            listDecorationBuildings.Add(building.buildingPrefab);
                            break;
                        case BuildingType.Defense:
                            contentUIDefenseInstance.textMeshPro.text = "Defense"; contentUIDefenseInstance.InstantiateIcon();
                            contentUIDefenseInstance.SetImage(building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                            contentUIDefenseInstance.SetResource(building.keyValuePairs);
                            contentUIDefenseInstance.SetInformation(infoInstance, building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite,
                                building.buildingName, building.description);
                            listDefenseBuildings.Add(building.buildingPrefab);
                            break;
                        
                    }
                }
                else
                {
                    Debug.LogWarning("ContentUI or textMeshPro is not assigned on the shopContentUI prefab!");
                }
            }

            Debug.Log($"Categorized buildings: {buildingObjects.buildings.Count} total");
            Debug.Log($"Production: {listProductionBuildings.Count}");
            Debug.Log($"Beds: {listBedBuildings.Count}");
            Debug.Log($"Towers: {listTowerBuildings.Count}");
            Debug.Log($"Decorations: {listDecorationBuildings.Count}");
            Debug.Log($"Defense: {listDefenseBuildings.Count}");
            Debug.Log($"Main Base: {listMainBaseBuildings.Count}");
        }

        public void SetActiveInfomation(bool isActive)
        {
            if (infoInstance != null)
            {
                infoInstance.SetActive(isActive);
            }
            else
            {
                Debug.LogWarning("Info instance is not assigned or instantiated.");
            }
        }


    }

}