using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class ActionData
{
    public Transform target;
    public Vector3 targetPosition;
    public float startTime;
    public float lastActionTime;
    public Dictionary<string, object> customData = new Dictionary<string, object>();

    public T GetCustomData<T>(string key, T defaultValue = default(T))
    {
        if (customData.ContainsKey(key) && customData[key] is T)
            return (T)customData[key];
        return defaultValue;
    }

    public void SetCustomData<T>(string key, T value)
    {
        customData[key] = value;
    }
}