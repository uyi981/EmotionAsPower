using System.Collections;
using System.Collections.Generic;
using LgTyUtils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;
using Assets.__EmotionAsPower.Scripts.Gameplay.UIShop;
public class IconUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Prefab Spawn")]
    public GameObject iconResourcePrefab;
    [Header("Referece Gameobject")]
    public Image buildingIcon;
    public GameObject resource;
    public ShopSystem shopSystem;
    [Header("Icon Building Infomation")]
    public int buildingID;
    public string buildingName;
    public string buildingDescription;


    private GameObject iconResourcePrefabInstance;
    private Button button;
    private PlacementSystem placementSystem;

    List<IconResource> iconResources = new List<IconResource>();


    private void Start()
    {
        placementSystem = Singleton<PlacementSystem>.Instance;
        shopSystem = Singleton<ShopSystem>.Instance;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    if (information != null)
    //    {
    //        information.SetActive(false);
    //    }
    //}

    public void OnPointerClick(PointerEventData eventData)
    {
        // Xử lý sự kiện click ở đây
        //Debug.Log($"Đã click vào: {buildingName}");

        Singleton<ShopSystem>.Instance.SetGridFrame(GetComponent<Image>());
        if (shopSystem.info == null)
        {
            Debug.LogWarning("Information panel is not assigned in the inspector!");
            return;
        }
        shopSystem.info.gameObject.SetActive(true);
        shopSystem.info.SetInformation(buildingIcon, buildingName, buildingDescription, resource, this);

    }
    public void Place()
    {
        if (placementSystem != null)
        {
            Building building = Singleton<PlacementSystem>.Instance.database.buildings[buildingID];
            if (Singleton<ItemStorage>.Instance.CheckListRequireItem(building.keyValuePairs))
            {
                placementSystem.StartPlacement(buildingID);
                Singleton<ShopSystem>.Instance.CloseShop();
            }
        }
        else
        {
            Debug.LogWarning("PlacementSystem not found in the scene!");
        }
    }









    public void SetResource(SerializableDictionary<ItemSO, int> resourceData)
    {
        if (resourceData != null)
        {
            // Set up new resources
            foreach (var item in resourceData)
            {
                Debug.Log($"Setting resource: {item.Key} with amount: {item.Value}");
                iconResourcePrefabInstance = Instantiate(iconResourcePrefab, resource.transform);
                var itemIcon = iconResourcePrefabInstance.GetComponent<IconResource>();
                if (itemIcon != null && item.Key != null)
                {
                    itemIcon.Setup(item.Key, item.Value);
                    iconResources.Add(itemIcon);
                }
            }
        }
        else
        {
            Debug.LogWarning("Cannot set resource: iconResourcePrefabInstance is null or resource data is invalid");
        }

        SerializableDictionary<ItemSO, int> missingItem = Singleton<ItemStorage>.Instance.CheckListRequireItemAndReturnItemMissing(resourceData);
        if (missingItem.Count > 0)
        {
            buildingIcon.color = Color.red; // Set building icon color to red if any item is missing
            for (int k = 0; k < iconResources.Count; k++)
            {
                if (missingItem.ContainsKey(iconResources[k].item))
                {
                    iconResources[k].image.color = Color.red; // Set color to red if the item is missing
                }
            }
        }
    }


    public void SetIconData(int id, Sprite img, string name, string des)
    {
        try
        {
            buildingID = id;
            buildingDescription = des ?? string.Empty;
            buildingName = name ?? string.Empty;
            buildingIcon.sprite = img;
            Debug.Log($"SetInformation called with: Name={name}, Description={des}, Image={img?.name}");
            return;



        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SetInformation: {e.Message}");
        }
    }

 
}

