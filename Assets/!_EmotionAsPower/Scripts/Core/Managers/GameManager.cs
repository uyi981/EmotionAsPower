using System;
using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Action OnSetupFinished;

    protected override void Awake()
    {
        SetupAll();
    }

    private IEnumerator SetupAll()
    {
        yield return ContentManager.Instance.SetupCoroutine();
        UIManager.Instance.Setup();
        OnSetupFinished?.Invoke();
    }
}
