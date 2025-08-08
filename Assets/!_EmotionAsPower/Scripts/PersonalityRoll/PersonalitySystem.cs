using System.Collections.Generic;
using UnityEngine;

public class PersonalitySystem : Singleton<PersonalitySystem>
{
    public List<PersonalitySO> personalities = new List<PersonalitySO>();
    public Dictionary<string,PersonalitySO> personalitiesDic = new Dictionary<string,PersonalitySO>();
    public void SetUp()
    {
        foreach(var item in personalities)
        {
            personalitiesDic.Add(item.name, item);
        }
    }
    public PersonalitySO GetPersonality(string name)
    {
        if (personalitiesDic.TryGetValue(name, out var personality))
        {
            return personality;
        }
        Debug.LogError($"Personality with name {name} not found.");
        return null;
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
