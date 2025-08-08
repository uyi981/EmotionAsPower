
using System.Collections.Generic;
using UnityEngine;

public class VFXPool
{
    Stack<GameObject> objects = new Stack<GameObject>();
    public float lastUsedTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject GetSkillObject()
    {
        lastUsedTime = Time.time;
        if (objects.Count > 0)
        {
            return objects.Pop();
        }
        return null;
    }
    public void ReturnSkillObject(GameObject skillObject)
    {
        lastUsedTime = Time.time;
        if (skillObject != null)
        {
            skillObject.SetActive(false);
            objects.Push(skillObject);
        }
    }
    public void Clear()
    {
        while (objects.Count > 0)
        {
            GameObject.Destroy(objects.Pop());
        }
    }
} 

