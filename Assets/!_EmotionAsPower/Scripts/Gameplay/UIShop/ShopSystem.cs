using System.Collections;
using System.Collections.Generic;
using Assets.__EmotionAsPower.Scripts.Gameplay.Building.BuidingChild;
using Assets.__EmotionAsPower.Scripts.Gameplay.UIShop.Content;
using LgTyUtils;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.UIShop
{
    public class ShopSystem : Singleton<ShopSystem>
    {
        [Header("Building Lists")]
        public List<GameObject> listProductionBuildings = new List<GameObject>();
        public List<GameObject> listBedBuildings = new List<GameObject>();
        public List<GameObject> listTowerBuildings = new List<GameObject>();
        public List<GameObject> listDecorationBuildings = new List<GameObject>();
        public List<GameObject> listDefenseBuildings = new List<GameObject>();
        public List<GameObject> listFoodFactoryBuildings = new List<GameObject>();
        public List<GameObject> listBreedingBuildings = new List<GameObject>();
        public List<GameObject> listSpecialBuildings = new List<GameObject>(); // If you have special buildings, uncomment this line
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
        GameObject foodFactoryBuilding;
        GameObject breedingBuilding;
        GameObject specialBuilding;
        GameObject infoInstance;


        private void Start()
        {
            // Instantiate the shop content UI prefab
            productionBulding = Instantiate(buldingContentUI, contentManager.transform);
            towerBuilding = Instantiate(buldingContentUI, contentManager.transform);
             bedBuilding = Instantiate(buldingContentUI, contentManager.transform);
           decorationBuilding = Instantiate(buldingContentUI, contentManager.transform);
            defenseBuilding = Instantiate(buldingContentUI, contentManager.transform);
            foodFactoryBuilding = Instantiate(buldingContentUI, contentManager.transform);
            breedingBuilding = Instantiate(buldingContentUI, contentManager.transform);
            specialBuilding = Instantiate(buldingContentUI, contentManager.transform);
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
            listFoodFactoryBuildings.Clear();
            listBreedingBuildings.Clear();
            listSpecialBuildings.Clear();
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
                ContentUI contentUIFoodFactoryInstance = foodFactoryBuilding.GetComponent<ContentUI>();
                ContentUI contentUIInstanceBreeding = breedingBuilding.GetComponent<ContentUI>();
                ContentUI contentUISpecialInstance = specialBuilding.GetComponent<ContentUI>();


                // Set the TextMeshProUGUI text based on building type
                if (contentUIInstance != null && contentUIInstance.textMeshPro != null)
                {
                    switch (building.type)
                    {
                        case BuildingType.Production:
                            contentUIInstance.textMeshPro.text = building.type.ToString();
                            contentUIInstance.InstantiateIcon();
                            contentUIInstance.SetImage(building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                            contentUIInstance.SetResource(building.keyValuePairs);
                            contentUIInstance.SetInfo(building.buildingID,building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite,
                                building.buildingName, building.description);
                            contentUIInstance.IconUIInstance.GetComponent<IconUI>().Information = infoInstance;


                            //Debug.Log($"Adding production buildinggggggggggggggggggggggg: " + building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite.name);
                            listProductionBuildings.Add(building.buildingPrefab);
                            break;
                        case BuildingType.Housing:
                            contentUIBedInstance.textMeshPro.text = building.type.ToString();
                            contentUIBedInstance.InstantiateIcon();
                            contentUIBedInstance.SetImage(building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                            contentUIBedInstance.SetResource(building.keyValuePairs);
                            contentUIBedInstance.SetInfo(building.buildingID, building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite,
                                building.buildingName, building.description);
                            contentUIBedInstance.IconUIInstance.GetComponent<IconUI>().Information = infoInstance;
                            listBedBuildings.Add(building.buildingPrefab);
                            break;
                        case BuildingType.Tower:
                            contentUITowerInstance.textMeshPro.text = building.type.ToString();
                            contentUITowerInstance.InstantiateIcon();
                            contentUITowerInstance.SetImage(building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                            contentUITowerInstance.SetResource(building.keyValuePairs);
                           
                            contentUITowerInstance.SetInfo(building.buildingID, building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite,
                                building.buildingName, building.description);
                            contentUITowerInstance.IconUIInstance.GetComponent<IconUI>().Information = infoInstance;
                            listTowerBuildings.Add(building.buildingPrefab);
                            break;
                        case BuildingType.Entertainment:
                            contentUIDecorationInstance.textMeshPro.text = building.type.ToString();
                            contentUIDecorationInstance.InstantiateIcon();
                            contentUIDecorationInstance.SetImage(building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                            contentUIDecorationInstance.SetResource(building.keyValuePairs);
                            contentUIDecorationInstance.SetInfo(building.buildingID, building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite,
                                building.buildingName, building.description);
                            contentUIDecorationInstance.IconUIInstance.GetComponent<IconUI>().Information = infoInstance;
                            listDecorationBuildings.Add(building.buildingPrefab);
                            break;
                        case BuildingType.Defense:
                            contentUIDefenseInstance.textMeshPro.text = building.type.ToString();
                            contentUIDefenseInstance.InstantiateIcon();
                            contentUIDefenseInstance.SetImage(building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                            contentUIDefenseInstance.SetResource(building.keyValuePairs);
                      
                            contentUIDefenseInstance.SetInfo(building.buildingID, building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite,
                                building.buildingName, building.description);
                            contentUIDefenseInstance.IconUIInstance.GetComponent<IconUI>().Information = infoInstance;
                            listDefenseBuildings.Add(building.buildingPrefab);
                            break;
                         case BuildingType.FoodFactory:
                            contentUIFoodFactoryInstance.textMeshPro.text = building.type.ToString();
                            contentUIFoodFactoryInstance.InstantiateIcon();
                            contentUIFoodFactoryInstance.SetImage(building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                            contentUIFoodFactoryInstance.SetResource(building.keyValuePairs);
                            contentUIFoodFactoryInstance.SetInfo(building.buildingID, building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite,
                                building.buildingName, building.description);
                            contentUIFoodFactoryInstance.IconUIInstance.GetComponent<IconUI>().Information = infoInstance;
                            listFoodFactoryBuildings.Add(building.buildingPrefab);
                            break;

                        case BuildingType.Breeding:
                            contentUIInstanceBreeding.textMeshPro.text = building.type.ToString();
                            contentUIInstanceBreeding.InstantiateIcon();
                            contentUIInstanceBreeding.SetImage(building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                            contentUIInstanceBreeding.SetResource(building.keyValuePairs);
                            contentUIInstanceBreeding.SetInfo(building.buildingID, building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite,
                                building.buildingName, building.description);
                            contentUIInstanceBreeding.IconUIInstance.GetComponent<IconUI>().Information = infoInstance;
                            //listMainBaseBuildings.Add(building.buildingPrefab);
                            break;
                        case BuildingType.Special:
                            contentUISpecialInstance.textMeshPro.text = building.type.ToString();
                            contentUISpecialInstance.InstantiateIcon();
                            contentUISpecialInstance.SetImage(building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite);
                            contentUISpecialInstance.SetResource(building.keyValuePairs);
                            contentUISpecialInstance.SetInfo(building.buildingID, building.buildingPrefab.GetComponentInChildren<SpriteRenderer>().sprite,
                                building.buildingName, building.description);
                            contentUISpecialInstance.IconUIInstance.GetComponent<IconUI>().Information = infoInstance;
                            listSpecialBuildings.Add(building.buildingPrefab);
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

        public void CloseShop()
        {
                gameObject.SetActive(false);
            
        }


    }

}