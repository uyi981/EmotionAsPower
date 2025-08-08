using Assets.__EmotionAsPower.Scripts.Gameplay.Building.BuidingChild;
using TMPro;
using UnityEngine;

public class BreedingUI : MonoBehaviour
{
    string name = "";
    public BreedingBuilding breedingBuilding;
    public TextMeshProUGUI nameInputField;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void CreateVillage()
    {
        if(breedingBuilding == null)
        {
            Debug.LogError("BreedingBuilding is null!");
            return;
        }
      //  breedingBuilding.Breading();
    }
    public void SetBreedingBuilding(BreedingBuilding breedingBuilding)
    {
        if (breedingBuilding == null)
        {
            Debug.LogError("BreedingBuilding is null!");
            return;
        }
        this.breedingBuilding = breedingBuilding;        
    }
}
