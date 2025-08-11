using UnityEngine;

public class DebugPanel : MonoBehaviour
{

    public void UpgradePlayerBase()
    {
        PlayerBase.Instance.Upgrade();
    }

    public void DebugSave()
    {
        DataPersistenceManager.Instance.SaveGame();
    }

    public void DebugLoad()
    {
        DataPersistenceManager.Instance.LoadGame();
    }
}