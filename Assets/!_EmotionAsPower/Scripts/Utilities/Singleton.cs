using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.IO.LowLevel.Unsafe;
public class Singleton<T> : UnityEngine.MonoBehaviour where T : Component
{
    private static T instance;
    private static bool isShuttingDown = false;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            if (isShuttingDown)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                return null;
            }

            lock (_lock)
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<T>();

                    if (instance == null)
                    {
                        Debug.LogError($"[Singleton] No instance of {typeof(T)} found in the scene.");
                    }
                }

                return instance;
            }
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

    protected virtual void OnApplicationQuit()
    {
        isShuttingDown = true;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            isShuttingDown = true;
            instance = null;
        }
    }
}