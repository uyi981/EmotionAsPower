using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.IO.LowLevel.Unsafe;
public class Singleton<T> : UnityEngine.MonoBehaviour where T : Component
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();
                if (instance == null)
                {
                    Debug.LogError($"No {typeof(T)} instance found in the scene");
                }
            }
            return instance;
        }
    }
    protected virtual void Awake()
    {
        // Check for multiple instances
        if (instance == null)
        {
            instance = this as T;
        } 
        else
        {
            if (instance != this)
            {
                Debug.LogError($"Multiple {typeof(T)} instances were found: {this.gameObject.name}, {instance.gameObject.name}. Only one instance is allowed");
            }
        }
    }

    protected virtual void OnDestroy() { 
        if(instance == this)
        {
            instance = null;
        }
    } 
}