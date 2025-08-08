using System;
using LgTyUtils;
using UnityEngine;

public class PlayerBase : Singleton<PlayerBase>, IDataPersistence, IHealth
{
    private SerializableDictionary<int, PlayerBaseLevel> playerBaseLevelConfig;

    [SerializeField]
    private int level;

    [SerializeField]
    private float health = 100f;

    private bool isDestroyed = false;

    [SerializeField]
    private LevelUnlockedContents unlockedContents;

    public Action<int> OnLevelUpdate;
    public Action OnPlayerBaseDestroyed;

    public int Level => level;
    public float Health => health;
    public bool IsDestroyed => isDestroyed;


    private void Start()
    {
        //GameManager.Instance.OnSetupFinished += Initialize;
        Initialize();
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
        if(playerBaseLevelConfig == null)
        {
            playerBaseLevelConfig = ContentManager.Instance.playerBaseLevelConfig;
        }
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

    public void TakeDamage(float damage)
    {
        this.health -= damage;
        if (this.health <= 0) { 
            isDestroyed = true;
            OnPlayerBaseDestroyed?.Invoke();
        }
    }

    public bool IsDead()
    {
        return isDestroyed;
    }


}