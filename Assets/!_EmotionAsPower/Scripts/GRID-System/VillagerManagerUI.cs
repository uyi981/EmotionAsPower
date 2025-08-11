using System.Collections.Generic;
using UnityEngine;

public class VillagerManagerUI : MonoBehaviour
{
    public List<VillagerSlotUI> villagerSlots = new List<VillagerSlotUI>();
    public Sprite defaultVillagerIcon;
    public Sprite defaultVillagerIconSad;
    public Sprite defaultVillagerIconJoy;
    public Sprite defaultVillagerIconAngry;
    public Sprite defaultVillagerIconNeutral;
    public Sprite defaultVillagerIconFear;
    public Dictionary<Emotion, Sprite> emotionIcons = new Dictionary<Emotion, Sprite>();
    bool isSetUp = false;   
    public void UpdateVillagerSlots(List<Villager> villagerList)
    {
      SetUp();
      for ( int i = 0; i < villagerSlots.Count; i++)
      {
          if (i < villagerList.Count)
          {
              villagerSlots[i].SetVillager(villagerList[i],this);
              villagerSlots[i].gameObject.SetActive(true);
            }
          else
          {
              villagerSlots[i].SetVillager(null,this);
          }
        }
    }
    private void Start()
    {
        SetUp();
    }
    void SetUp()
    {
        if(isSetUp) return;
        foreach (Transform slot in gameObject.transform)
        {
            villagerSlots.Add(slot.GetComponent<VillagerSlotUI>());
            slot.gameObject.SetActive(false);
        }
        emotionIcons.Add(Emotion.Sad, defaultVillagerIconSad);
        emotionIcons.Add(Emotion.Joy, defaultVillagerIconJoy);
        emotionIcons.Add(Emotion.Anger, defaultVillagerIconAngry);
        emotionIcons.Add(Emotion.Normal, defaultVillagerIconNeutral);
        emotionIcons.Add(Emotion.Fear, defaultVillagerIconFear);
        isSetUp = true;
    }    
}
