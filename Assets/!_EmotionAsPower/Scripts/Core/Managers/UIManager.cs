using UnityEngine;

public class UIManager : Singleton<UIManager>, ISetup
{
    private bool showUI = false;

    public bool ShowUI => showUI;
    public void Setup()
    {
        showUI = true;
    }
}