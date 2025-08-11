using System;
using System.Collections;
using LgTyUtils;
using UnityEngine;

public class PlayerBase : Singleton<PlayerBase>, IDataPersistence, IHealth
{
    private SerializableDictionary<int, PlayerBaseLevel> playerBaseLevelConfig;

    [SerializeField]
    private int level;

    [SerializeField]
    private float maxHealth = 100f;
    private float health = 100f;

    private bool isDestroyed = false;
    private NewHealthBar healthBar;

    [SerializeField]
    private LevelUnlockedContents unlockedContents;

    [Header("Health Recovery")]
    [SerializeField] private float recoveryAmount = 5f;       // HP recovered each interval
    [SerializeField] private float recoveryInterval = 2f;     // Seconds between recovery ticks
    private Coroutine recoveryCoroutine;

    public Action<int> OnLevelUpdate;
    public Action OnPlayerBaseDestroyed;

    public int Level => level;
    public float Health => health;
    public bool IsDestroyed => isDestroyed;

    private void Start()
    {
        health = maxHealth;
        Initialize();

        healthBar = GetComponentInChildren<NewHealthBar>();
        if (healthBar != null)
        {
            healthBar.SetProcess(health / maxHealth);
        }

        // Start recovery loop
        recoveryCoroutine = StartCoroutine(HealthRecoveryLoop());
    }

    public void Initialize()
    {
        playerBaseLevelConfig = ContentManager.Instance.playerBaseLevelConfig;
        ValidateLevelConfig();
        OnLevelUpdate?.Invoke(level);
    }

    private void ValidateLevelConfig()
    {
        foreach (var level in playerBaseLevelConfig)
        {
            if (level.Key != level.Value.level)
            {
                Debug.LogWarning("The player base level config looks like having problem of level matching");
            }
        }
    }

    private bool ValidateLevel(int level)
    {
        if (playerBaseLevelConfig == null)
        {
            playerBaseLevelConfig = ContentManager.Instance.playerBaseLevelConfig;
        }
        return playerBaseLevelConfig.ContainsKey(level);
    }

    public void SetLevel(int level)
    {
        if (!ValidateLevel(level)) { return; }
        this.level = level;
        unlockedContents.AddUnlockedContents(playerBaseLevelConfig[level].unlockedContents);
        OnLevelUpdate?.Invoke(level);
    }

    public bool HasEnoughLevel(int level) => this.level >= level;

    public PlayerBaseLevel GetNextLevel()
    {
        if (!playerBaseLevelConfig.ContainsKey(level + 1))
        {
            Debug.LogWarning("Player has reached the maximum level");
            return null;
        }
        return playerBaseLevelConfig[level + 1];
    }

    public void LoadGame(GameData gameData)
    {
        this.level = 0;
        while (level < gameData.playerBaseLevel)
        {
            SetLevel(++level);
        }
    }

    public void SaveGame(ref GameData gameData)
    {
        gameData.playerBaseLevel = this.Level;
    }

    public void Upgrade()
    {
        SetLevel(Level + 1);
    }

    public void TakeDamage(float damage)
    {
        this.health -= damage;
        if (healthBar != null)
        {
            healthBar.SetProcess(health / maxHealth);
        }

        if (this.health <= 0)
        {
            isDestroyed = true;
            OnPlayerBaseDestroyed?.Invoke();

            // Stop recovery if destroyed
            if (recoveryCoroutine != null)
            {
                StopCoroutine(recoveryCoroutine);
            }
        }
    }

    public bool IsDead() => isDestroyed;

    private IEnumerator HealthRecoveryLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(recoveryInterval);

            if (!isDestroyed && health < maxHealth)
            {
                health += recoveryAmount;
                if (health > maxHealth) health = maxHealth;

                if (healthBar != null)
                {
                    healthBar.SetProcess(health / maxHealth);
                }
            }
        }
    }
}
