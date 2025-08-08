using Assets.__EmotionAsPower.Scripts.Gameplay.UIShop;
using UnityEngine;
using UnityEngine.UI;
public class DetailInfoController : Singleton<DetailInfoController>
{
    public VillagerDetailUI villagerUI;
    public OptionFunctionUI building;
    public GameObject button;
    public Image image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OpenVillageUI(Villager villager)
    {
        button.SetActive(true); // Show the button when opening the UI
        image.enabled = true; // Show the image when opening the UI
        villagerUI.gameObject.SetActive(true);
        villagerUI.ReceiveVillagerData(villager);
        building.gameObject.SetActive(false);
    }
    public void OpenBuildingUI(BuildingBase buildingBase)
    {
        button.SetActive(true); // Show the button when opening the UI
        image.enabled = true; // Show the image when opening the UI
        building.gameObject.SetActive(true);
        building.UpdateUI(buildingBase);
        villagerUI.gameObject.SetActive(false);
    }
    private void Start()
    {
        image.enabled = false; // Ensure the image is not visible at the start
        villagerUI.gameObject.SetActive(false);
        building.gameObject.SetActive(false);
        button.SetActive(false); // Hide the button at the start

    }
    public void CloseUI()
    {
        villagerUI.gameObject.SetActive(false);
        building.gameObject.SetActive(false);
        image.enabled = false;
        button.SetActive(false); // Hide the button when closing the UI
    }
}
