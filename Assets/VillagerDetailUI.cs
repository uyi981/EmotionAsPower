using TMPro;
using UnityEngine;

public class VillagerDetailUI :MonoBehaviour
{
    public Villager currentSelectedVillager; // Reference to the Villager script
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI villagerNameText;
    public TextMeshProUGUI villagerStateText;
    public TextMeshProUGUI villagerHungerText;
    public TextMeshProUGUI villagerThirstText;
    public TextMeshProUGUI villagerPersonalityText;
    public TextMeshProUGUI villagerEmotion;
    public TextMeshProUGUI villagerPersonalityDescriptionText;
    public TextMeshProUGUI villagerHPText;
    public void UpdateUI()
    {
        villagerNameText.text = "NPC";
        villagerPersonalityText.text = "Personality: " + currentSelectedVillager.personality.name;
       // villagerHungerText.text = "Hunger: " + (int)currentSelectedVillager.currentHunger + "/100";
        villagerEmotion.text = "Emotion: " + currentSelectedVillager.currentEmotion.ToString() +" " + (int)currentSelectedVillager.emotion.GetEmotionMaxPoint() + "/100";
        villagerThirstText.text = "Thirst: " + "100/100";
        villagerStateText.text = "State: " + currentSelectedVillager.currentStateName;
        villagerPersonalityDescriptionText.text = currentSelectedVillager.personality.description;
        villagerHPText.text = "HP: " + "100/100";
        // You can add more details as needed
    }
    public void ReceiveVillagerData(Villager villager)
    {
        if(currentSelectedVillager != null)
        {
            // Unsubscribe from the previous villager's update event
            currentSelectedVillager.OnVillagerUpdate -= UpdateUI;
        }
        this.currentSelectedVillager = villager;
        villager.OnVillagerUpdate += UpdateUI;
        UpdateUI();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
