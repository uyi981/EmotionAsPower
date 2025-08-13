using System.Collections;
using UnityEngine;

public class PrisonBuilding : BuildingBase
{
    public Villager currentPrison;
    public float timeCheckPrison = 30f;
    public Transform position;
    public void TakeEmotion(Emotion emotion,int value)
    {
        string id = EmotionHelper.GetEmotionID(emotion);
        Singleton<ItemStorage>.Instance.AddItem(id, value);
        string vfx = EmotionVFXHelper.GetVFXName(emotion);
        GameObject obj = Singleton<VFXPoolManager>.Instance.PopSKillObject(vfx);
        if (obj != null)
        {
            obj.transform.position = transform.position+Vector3.up*0.1f;
            obj.SetActive(true);
        }
        else
        {
            Debug.LogError("VFX object is null");
        }
    }
    public override void OnBuildingComplete()
    {
        base.OnBuildingComplete();
        InvokeRepeating(nameof(TakeEmotionEverytime), 0f, timeCheckPrison); // Check every second

    }
    private void Start()
    {
        base.Start();
        gameObject.tag = "PrisonBuilding";
    }
    public void TakeEmotionEverytime()
    {
        if(currentPrison == null)
        {
            return;
        }
        Emotion emotion = currentPrison.currentEmotion;
        float value = currentPrison.emotion.GetEmotionMaxPoint();
        if(emotion == Emotion.Normal)
        {
            ReleasePrison();//release prison
            //release prison
        }
        if (value < 20)
        {
            currentPrison.HandleEmotion(Emotion.Normal);
            ReleasePrison();//release prison
            return;
        }
        Singleton<ItemStorage>.Instance.AddItem(EmotionHelper.GetEmotionID(emotion), 10);
        currentPrison.emotion.minusEmotion(emotion,10);
        currentPrison.HandleEmotion(emotion);
    }
    public void SetPrison(Villager villager)
    {
        if (villager.emotion.Equals(Emotion.Normal))
        {
            return;
        }
        if (currentPrison != null)
        {
            Debug.LogWarning("Prison is already set. Please release the current villager before setting a new one.");
            return;
        }
        if (currentPrison == null)
        {
            if(villager.emotion.Equals(Emotion.Normal))
            {
                return;
            }
            currentPrison = villager;
            villager.transform.position =position.position;
        }
        currentPrison.TransitionTo(currentPrison.villagerPrisonState);
    }
    public void ReleasePrison()
    {
        currentPrison.isPrisoner = false; // Set villager as not a prisoner
        currentPrison.TransitionTo(currentPrison.villagerIdleState);
        currentPrison = null;
    }
}
public static class EmotionVFXHelper
{
    public static string GetVFXName(Emotion emotion)
    {
        string vfxName = string.Empty;
        switch (emotion)
      {
            case Emotion.Normal:
            vfxName = "";
            break;
        case Emotion.Joy:
            vfxName = "joy";
            break;
        case Emotion.Sad:
            vfxName = "sad";
            break;
        case Emotion.Anger:
            vfxName = "angry";
            break;
        case Emotion.Fear:
            vfxName = "fear";
            break;
        case Emotion.Apethatic:
            vfxName = "boring";
            break;
        default:
            Debug.LogError("Unknown emotion type");
            break;
        }
        return vfxName;
    }
}
