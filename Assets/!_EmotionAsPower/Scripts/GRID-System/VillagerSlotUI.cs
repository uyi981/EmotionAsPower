using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VillagerSlotUI : MonoBehaviour
{
    public Villager villager;
    public GameObject sad;
    public GameObject joy;
    public GameObject angry;
    public GameObject neutral;
    public GameObject fear;
    public GameObject boring;
    public GameObject icon;
    public Image border;
    public bool isSetup = false;
    Dictionary<Emotion, GameObject> emotionFace;
    VillagerManagerUI villagerManagerUI;
    public void UpdateVillagerData()
    {

    }
    private void Start()
    {
        SetUp();
    }
    public void UpdateEmotion()
    {

        SetUp();
        foreach (var face in emotionFace)
        {
            face.Value.SetActive(false);
        }
        emotionFace[villager.currentEmotion].SetActive(true);
        icon.GetComponent<Image>().sprite = villagerManagerUI.emotionIcons[villager.currentEmotion];
        switch (villager.currentEmotion)
        {
            case Emotion.Joy:
                // Ví dụ: NPC cười, vẫy tay, chạy nhanh
                border.color = Color.yellow;
                break;

            case Emotion.Sad:
                // Ví dụ: NPC chậm chạp, cúi đầu
                border.color = Color.lightBlue;
                break;

            case Emotion.Anger:
                // Ví dụ: NPC đỏ mặt, nói gắt, đấm tường
                border.color = Color.red;
                break;

            case Emotion.Fear:
                // Ví dụ: NPC rung, bỏ chạy, né xa player
                border.color = Color.limeGreen;
                break;

            case Emotion.Apethatic:
                // Ví dụ: NPC không phản ứng gì, đứng yên
                border.color = Color.gray;
                break;

            case Emotion.Normal:
            default:
                // NPC hoạt động bình thường
                border.color = Color.white;
                break;
        }
    }
    public void SetUp()
    {
        if (isSetup) return;
        emotionFace = new Dictionary<Emotion, GameObject>
        {
            { Emotion.Sad, sad },
            { Emotion.Joy, joy },
            { Emotion.Anger, angry },
            { Emotion.Normal, neutral },
            { Emotion.Fear, fear },
            { Emotion.Apethatic, boring }
        };
        isSetup = true;
    }
    public void SetVillager(Villager villager,VillagerManagerUI villagerManagerUI)
    {
        this.villagerManagerUI = villagerManagerUI;
        if (villager == null)
        {
            return;
        }
        if (this.villager != null)
        {
            this.villager.changeEmotion -= UpdateEmotion;
        }
        this.villager = villager;
        this.villager.changeEmotion += UpdateEmotion;

        UpdateVillagerData();
    }
    public void FocusToVillager()
    {
        Singleton<CameraController>.Instance.FocusMode(villager.transform.position);
    }
}
