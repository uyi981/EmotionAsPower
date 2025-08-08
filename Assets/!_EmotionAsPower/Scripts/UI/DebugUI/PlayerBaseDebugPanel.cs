using UnityEngine;

public class PlayerBaseDebugPanel : MonoBehaviour
{

    public void UpgradePlayerBase()
    {
        PlayerBase.Instance.Upgrade();
    }
}