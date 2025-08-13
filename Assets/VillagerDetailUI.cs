using System.Collections.Generic;
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
        SetStatsFromString(currentSelectedVillager.personality.description);
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
    public void SetStatsFromString(string multiLineText)
    {
        if (villagerPersonalityDescriptionText == null || string.IsNullOrWhiteSpace(multiLineText)) return;

        // Tách thành list string theo dòng
        string[] lines = multiLineText.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        List<string> stats = new List<string>(lines);
        ApplyColors(stats);
    }

    private void ApplyColors(List<string> stats)
    {
        string finalText = "";

        foreach (string stat in stats)
        {
            string trimmed = stat.Trim();

            if (trimmed.StartsWith("+"))
            {
                finalText += $"<color=green>{trimmed}</color>\n";
            }
            else if (trimmed.StartsWith("-"))
            {
                finalText += $"<color=red>{trimmed}</color>\n";
            }
            else
            {
                finalText += $"{trimmed}\n";
            }
        }

        villagerPersonalityDescriptionText.text = finalText.TrimEnd('\n');
    }
}
