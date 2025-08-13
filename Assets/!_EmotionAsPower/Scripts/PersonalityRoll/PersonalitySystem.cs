using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PersonalitySystem : Singleton<PersonalitySystem>
{
    public List<PersonalitySO> personalities = new List<PersonalitySO>();
    public List<EmotionSO> emotionModifiers = new List<EmotionSO>();
    public Dictionary<string,PersonalitySO> personalitiesDic = new Dictionary<string,PersonalitySO>();
    public Dictionary<Emotion, EmotionSO> emotionModifier = new Dictionary<Emotion, EmotionSO>();
    bool isSetUp = false;
    public void SetUp()
    {
        if(isSetUp)
            return;
        foreach (var item in personalities)
        {
            personalitiesDic.Add(item.name, item);
        }
        foreach (var item in emotionModifiers)
        {
            if (!emotionModifier.ContainsKey(item.id))
            {
                emotionModifier.Add(item.id, item);
            }
        }
        isSetUp = true;
    }
    private void Start()
    {
        SetUp();
    }
    public PersonalitySO GetPersonality(string name)
    {
        PersonalitySO personality = personalities.Find(p => p.name == name);
        if(personality!=null)
            return personality;
        return null;
    }
    public EmotionSO GetEmotionModifier(Emotion emotion)
    {
        return emotionModifier[emotion];
    }
    public PersonalitySO Breeding()
    {
        //int number = Random.Range(0, 100);
        //if(number < 40)
        //{
        //    return father;
        //}
        //else if(number<80)
        //{
        //    return mother;
        //}
        //else
        //{
          
        //}
        int i = Random.Range(0, personalities.Count);
        return personalities[i];
    }
}
