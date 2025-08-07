using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T instance;
    private static bool isShuttingDown = false;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            if (isShuttingDown) return null;

            lock (_lock)
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<T>();
                    if (instance == null)
                        Debug.LogWarning($"[Singleton] No active instance of {typeof(T)} found.");
                }

                return instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            //DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this)
        {
            Debug.LogWarning($"[Singleton] Duplicate detected for {typeof(T)}. Destroying {this.gameObject.name}");
            Destroy(this.gameObject);
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
            instance = null;
            isShuttingDown = false;
        }
    }
}
