using System;
using LgTyUtils;
using UnityEngine;

public class PlayerBase : Singleton<PlayerBase>, IDataPersistence
{
    private SerializableDictionary<int, PlayerBaseLevel> playerBaseLevelConfig;

    [SerializeField]
    private int level;
    public int Level => level;

    [SerializeField]
    private LevelUnlockedContents unlockedContents;

    public Action<int> OnLevelUpdate;

    private void Start()
    {
        GameManager.Instance.OnSetupFinished += Initialize;
    }

    public void Initialize()
    {
        playerBaseLevelConfig = ContentManager.Instance.playerBaseLevelConfig;

        ValidateLevelConfig();
        OnLevelUpdate?.Invoke(level);
    }

    private void ValidateLevelConfig()
    {
        foreach(var level in playerBaseLevelConfig)
        {
            if(level.Key != level.Value.level)
            {
                Debug.LogWarning("The player base level config looks like having problem of level matching");
            }
        }
    }

    private bool ValidateLevel(int level)
    {
        if (playerBaseLevelConfig.ContainsKey(level))
        {
            return true;
        }
        return false;
    }

    public void SetLevel(int level)
    {
        if(!ValidateLevel(level)) { return; }
        this.level = level;
        unlockedContents.AddUnlockedContents(playerBaseLevelConfig[level].unlockedContents);
        OnLevelUpdate?.Invoke(level);
    }

    public bool HasEnoughLevel(int level)
    {
        return this.level >= level;
    }

    public PlayerBaseLevel GetNextLevel(){
        if (!playerBaseLevelConfig.ContainsKey(level + 1)) {
            Debug.LogWarning("Player has reached the maximum level");
            return null;
        }
        return playerBaseLevelConfig[level + 1];
    }

    public void LoadGame(GameData gameData)
    {
        this.level = 0;
        while(level < gameData.playerBaseLevel)
        {
            SetLevel(++level);
        }
    }

    public void SaveGame(ref GameData gameData)
    {
        gameData.playerBaseLevel = this.Level;
    }

    // TODO: add check unlocked contents contain a content

    public void Upgrade()
    {
        SetLevel(Level + 1);
    }
}