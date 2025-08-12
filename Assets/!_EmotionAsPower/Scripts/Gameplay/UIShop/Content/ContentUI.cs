using System.Collections;
using LgTyUtils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContentUI : MonoBehaviour
{
    [Header("IconPrefab Spawn")]
    public GameObject iconUI;


    [Header("Reference Object")]
    public TextMeshProUGUI textMeshPro;
    public GameObject iconBuildings;




    private GameObject iconUIInstance;
    public TextMeshProUGUI TextMeshPro { get; set; }
    public GameObject IconBuildings { get; set; }
    public GameObject IconUIInstance { get => iconUIInstance; set => iconUIInstance = value; }

    
    public GameObject InstantiateIcon()
    {
        iconUIInstance = Instantiate(iconUI, iconBuildings.transform);
        //iconUIInstance.GetComponent<IconUI>().InstantiateResource();
        return iconUIInstance;
    }

    public void SetImage(Sprite sprite)
    {
        if (iconUIInstance == null)
        {
            Debug.LogError("iconUIInstance is null in SetImage. Make sure InstantiateIcon() is called first.");
            return;
        }

        var iconImage = iconUIInstance.GetComponent<IconUI>();
        if (iconImage != null && iconImage.buildingIcon != null)
        {
            iconImage.buildingIcon.sprite = sprite;
        }
        else
        {
            Debug.LogError("IconUI component or buildingIcon is not properly set up on the iconUIInstance prefab.");
        }
    }

    public void SetResource(SerializableDictionary<ItemSO, int> resources)
    {
        // Set up new resources
        iconUIInstance.GetComponent<IconUI>().SetResource(resources);
    }

    public void SetInfo(int id, Sprite img, string name, string des)
    {
        var iconUI = iconUIInstance.GetComponent<IconUI>();
        iconUI.SetIconData(id, img, name, des);
    }





}
