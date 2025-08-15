using TMPro;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(ItemDropper))]
[RequireComponent(typeof(ShakeEffect))]
public class Resource : MonoBehaviour, IInteractable
{
    [Header("Resource Properties")]
    [SerializeField]
    private string id;
    public string ID => id;

    [SerializeField]
    private string displayName;
    public string DisplayName => displayName;

    [Header("Resource Material")]
    [SerializeField]
    private string textureProperty = "_MainTexure";

    [Header("Harvest Settings")]
    [SerializeField]
    private bool canBeHarvested = true;
    [SerializeField]
    private float harvestCooldown = 1f;
    [SerializeField]
    private bool destroyWhenDepleted = true;
    [SerializeField]
    private bool showHealthUI = true;

    [Header("Harvest Marker")]
    [SerializeField]
    private GameObject harvestMarker;

    [Header("Harvest Sound Settings")]
    [SerializeField]
    private bool enableHarvestSound = true;
    [SerializeField]
    private string harvestSoundID = "harvest_sound";
    [SerializeField]
    private bool enableDepletedSound = true;
    [SerializeField]
    private string depletedSoundID = "resource_depleted";
    [SerializeField]
    private bool enableRespawnSound = false;
    [SerializeField]
    private string respawnSoundID = "resource_respawn";
    [SerializeField]
    private float minDamageForSound = 1f;
    [SerializeField]
    private float soundCooldown = 0.1f;
    [SerializeField]
    private bool randomizePitch = true;
    [SerializeField]
    private Vector2 pitchRange = new Vector2(-0.1f, 0.1f);

    [Header("Regular Drop Settings")]
    [SerializeField]
    private bool enableRegularDrops = false;
    [SerializeField]
    private float regularDropInterval = 30f;
    [SerializeField]
    private DropableItem[] regularDropItems;
    [SerializeField]
    private bool dropOnlyWhenAlive = true;

    [Header("Events")]
    public UnityEvent OnResourceHarvested;
    public UnityEvent OnResourceDepleted;
    public UnityEvent OnResourceRespawned;
    public UnityEvent OnRegularDrop;

    // Components
    private Health health;
    private ItemDropper itemDropper;
    private ShakeEffect shakeEffect;
    private TextMeshProUGUI healthText;
    private bool isInitialized = false;
    private float lastHarvestTime = 0f;
    private float lastRegularDropTime = 0f;
    private bool isForHarvest = false;
    private float lastSoundTime = 0f;

    // Properties
    public Health Health => health;
    public ItemDropper ItemDropper => itemDropper;
    public ShakeEffect ShakeEffect => shakeEffect;
    public bool CanHarvest => canBeHarvested && !health.IsDead && Time.time >= lastHarvestTime + harvestCooldown;
    public bool IsDepleted => health != null && health.IsDead;
    public float LastRegularDropTime => lastRegularDropTime;
    public bool IsForHarvest => isForHarvest;

    private void Awake()
    {
        health = GetComponent<Health>();
        itemDropper = GetComponent<ItemDropper>();
        shakeEffect = GetComponent<ShakeEffect>();

        healthText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (!isInitialized)
        {
            Initialize();
        }
    }

    private void Update()
    {
        // Handle regular drops
        if (enableRegularDrops && ShouldDropRegularItems())
        {
            DropRegularItems();
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamageTaken.AddListener(OnResourceDamaged);
            health.OnDeath.AddListener(OnResourceDepletedInternal);
            health.OnRevived.AddListener(OnResourceRespawnedInternal);
        }

        // Connect shake effect to health component
        if (shakeEffect != null && health != null)
        {
            shakeEffect.ConnectToHealth(health);
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamageTaken.RemoveListener(OnResourceDamaged);
            health.OnDeath.RemoveListener(OnResourceDepletedInternal);
            health.OnRevived.RemoveListener(OnResourceRespawnedInternal);
        }

        // Disconnect shake effect
        if (shakeEffect != null)
        {
            shakeEffect.DisconnectFromHealth();
        }
    }

    public void Initialize()
    {
        if (health != null)
        {
            health.SetHealth(health.MaxHealth);
        }

        lastRegularDropTime = Time.time;

        UpdateHarvestMarkerVisibility();

        isInitialized = true;
    }

    public void InitializeWithSaveData(float savedHealth, float savedLastDropTime, bool savedIsForHarvest = false)
    {
        if (health != null)
        {
            health.SetHealth(savedHealth);
        }

        lastRegularDropTime = savedLastDropTime;
        isForHarvest = savedIsForHarvest;

        UpdateHarvestMarkerVisibility();
        UpdateResourceTag();

        isInitialized = true;
    }

    public void OnInteract()
    {
        // Get current mouse position from InputManager
        Vector2 mousePosition = InputManager.Instance.mousePos.ReadValue<Vector2>();

        // Show the resource info panel at mouse position
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowResourceInfoPanel(this, mousePosition);
            return;
        }
        Debug.Log("Clicked on Resource");
    }

    public InteractableType GetInteractableType() => InteractableType.Resource;

    public void ForceDropAllItems()
    {
        if (itemDropper != null)
        {
            itemDropper.DropFunction(transform.position, true);
        }
    }

    public void RespawnResource()
    {
        if (health != null)
        {
            health.Revive();
        }
    }

    public void DepleteResource()
    {
        if (health != null && !health.IsDead)
        {
            health.Die();
        }
    }

    public void TriggerShake()
    {
        if (shakeEffect != null)
        {
            shakeEffect.StartShake();
        }
    }

    // Called when the resource takes damage (harvested)
    private void OnResourceDamaged(float damageAmount)
    {
        PlayHarvestSound(damageAmount);
        lastHarvestTime = Time.time;
        OnResourceHarvested?.Invoke();
    }

    // Sound playing methods
    private void PlayHarvestSound(float damageAmount)
    {
        if (!enableHarvestSound || AudioManager.Instance == null) return;
        if (damageAmount < minDamageForSound) return;
        if (Time.time - lastSoundTime < soundCooldown) return;

        try
        {
            var soundBuilder = AudioManager.Instance.CreateSound()
                .WithSoundData(AudioManager.Instance.audioLibrarySO.GetSoundEffect(harvestSoundID))
                .WithPosition(transform.position);

            if (randomizePitch)
            {
                soundBuilder.WithRandomPitch();
            }

            soundBuilder.Play();
            lastSoundTime = Time.time;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to play harvest sound '{harvestSoundID}' for resource '{displayName}': {e.Message}");
        }
    }

    private void PlayDepletedSound()
    {
        if (!enableDepletedSound || AudioManager.Instance == null) return;

        try
        {
            var soundBuilder = AudioManager.Instance.CreateSound()
                .WithSoundData(AudioManager.Instance.audioLibrarySO.GetSoundEffect(depletedSoundID))
                .WithPosition(transform.position);

            if (randomizePitch)
            {
                soundBuilder.WithRandomPitch();
            }

            soundBuilder.Play();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to play depleted sound '{depletedSoundID}' for resource '{displayName}': {e.Message}");
        }
    }

    private void PlayRespawnSound()
    {
        if (!enableRespawnSound || AudioManager.Instance == null) return;

        try
        {
            var soundBuilder = AudioManager.Instance.CreateSound()
                .WithSoundData(AudioManager.Instance.audioLibrarySO.GetSoundEffect(respawnSoundID))
                .WithPosition(transform.position);

            if (randomizePitch)
            {
                soundBuilder.WithRandomPitch();
            }

            soundBuilder.Play();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to play respawn sound '{respawnSoundID}' for resource '{displayName}': {e.Message}");
        }
    }

    // Public methods to configure sound settings at runtime
    public void SetHarvestSoundEnabled(bool enabled)
    {
        enableHarvestSound = enabled;
    }

    public void SetHarvestSoundID(string soundID)
    {
        harvestSoundID = soundID;
    }

    public void SetDepletedSoundEnabled(bool enabled)
    {
        enableDepletedSound = enabled;
    }

    public void SetDepletedSoundID(string soundID)
    {
        depletedSoundID = soundID;
    }

    public void SetRespawnSoundEnabled(bool enabled)
    {
        enableRespawnSound = enabled;
    }

    public void SetRespawnSoundID(string soundID)
    {
        respawnSoundID = soundID;
    }

    public void SetMinDamageForSound(float minDamage)
    {
        minDamageForSound = Mathf.Max(0f, minDamage);
    }

    public void SetSoundCooldown(float cooldown)
    {
        soundCooldown = Mathf.Max(0f, cooldown);
    }

    public void SetPitchRandomization(bool enabled, Vector2 range = default)
    {
        randomizePitch = enabled;
        if (range != default(Vector2))
        {
            pitchRange = range;
        }
    }

    private bool ShouldDropRegularItems()
    {
        if (!enableRegularDrops || regularDropItems == null || regularDropItems.Length == 0)
            return false;

        if (dropOnlyWhenAlive && health.IsDead)
            return false;

        return Time.time >= lastRegularDropTime + regularDropInterval;
    }

    private void DropRegularItems()
    {
        lastRegularDropTime = Time.time;

        foreach (var dropableItem in regularDropItems)
        {
            if (dropableItem.item == null || dropableItem.amountChances == null)
                continue;

            int amountToDrop = DetermineDropAmount(dropableItem.amountChances);
            if (amountToDrop > 0)
            {
                DropSingleRegularItem(dropableItem.item, amountToDrop);
            }
        }

        OnRegularDrop?.Invoke();
    }

    private int DetermineDropAmount(AmountChance[] amountChances)
    {
        if (amountChances == null || amountChances.Length == 0)
            return 0;

        // Check each amount chance in order
        foreach (AmountChance amountChance in amountChances)
        {
            if (UnityEngine.Random.Range(0f, 100f) <= amountChance.chance)
            {
                return amountChance.amount;
            }
        }

        return 0; // Nothing drops
    }

    private void DropSingleRegularItem(ItemSO itemSO, int amount)
    {
        if (amount <= 0 || itemSO == null)
            return;

        float dropRadius = 1f;
        float dropForce = 3f;

        for (int i = 0; i < amount; i++)
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * dropRadius;
            Vector3 finalDropPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            GameObject droppedItem = ItemManager.Instance.SpawnItem(itemSO, 1, finalDropPosition);

            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 randomForce = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(0.5f, 1f),
                    UnityEngine.Random.Range(-1f, 1f)
                ).normalized * dropForce;

                rb.AddForce(randomForce, ForceMode.Impulse);
            }
        }
    }

    public void EnableRegularDrops(bool enable)
    {
        enableRegularDrops = enable;
        if (enable)
        {
            lastRegularDropTime = Time.time; // Reset timer when enabling
        }
    }

    public void SetRegularDropInterval(float interval)
    {
        regularDropInterval = Mathf.Max(1f, interval);
    }

    public void ForceRegularDrop()
    {
        if (regularDropItems != null && regularDropItems.Length > 0)
        {
            DropRegularItems();
        }
    }

    public float GetTimeUntilNextRegularDrop()
    {
        if (!enableRegularDrops) return -1f;
        return Mathf.Max(0f, lastRegularDropTime + regularDropInterval - Time.time);
    }

    public void SetForHarvest()
    {
        isForHarvest = true;
        UpdateHarvestMarkerVisibility();
        UpdateResourceTag();
    }

    public void UnsetForHarvest()
    {
        isForHarvest = false;
        UpdateHarvestMarkerVisibility();
        UpdateResourceTag();
    }

    private void UpdateHarvestMarkerVisibility()
    {
        if (harvestMarker != null)
        {
            harvestMarker.SetActive(isForHarvest);
        }
    }

    private void UpdateResourceTag()
    {
        gameObject.tag = isForHarvest ? "Resource" : "Untagged";
    }

    private void OnResourceDepletedInternal()
    {
        PlayDepletedSound();
        itemDropper.DropItems();
        OnResourceDepleted?.Invoke();

        if (destroyWhenDepleted)
        {
            // Destroy after a short delay to allow for effects/animations
            Invoke(nameof(DestroyResource), 0.5f);
        }
    }

    private void OnResourceRespawnedInternal()
    {
        PlayRespawnSound();
        // Reset regular drop timer when respawning
        lastRegularDropTime = Time.time;
        OnResourceRespawned?.Invoke();
    }

    private void DestroyResource()
    {
        if (UIManager.Instance.resourceInfoPanel.Resource == this)
        {
            UIManager.Instance.resourceInfoPanel.Hide();
        }
        Destroy(gameObject);
    }

    public void SetHarvestCooldown(float cooldown)
    {
        harvestCooldown = Mathf.Max(0f, cooldown);
    }

    public void SetCanBeHarvested(bool canHarvest)
    {
        canBeHarvested = canHarvest;
    }

    public float GetRemainingCooldown()
    {
        return Mathf.Max(0f, lastHarvestTime + harvestCooldown - Time.time);
    }

    public float GetHealthPercentage()
    {
        return health != null ? health.HealthPercentage : 0f;
    }

    private void OnValidate()
    {
        harvestCooldown = Mathf.Max(0f, harvestCooldown);
        regularDropInterval = Mathf.Max(1f, regularDropInterval);
        minDamageForSound = Mathf.Max(0f, minDamageForSound);
        soundCooldown = Mathf.Max(0f, soundCooldown);

        // Ensure pitch range is valid
        if (pitchRange.x > pitchRange.y)
        {
            pitchRange = new Vector2(pitchRange.y, pitchRange.x);
        }

        if (!string.IsNullOrEmpty(textureProperty) && !textureProperty.StartsWith("_"))
        {
            textureProperty = "_" + textureProperty;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Show harvest range if item dropper exists
        if (itemDropper != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }

        // Show regular drop indicator
        if (enableRegularDrops)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }
    }

    public void OnMouseDown()
    {
        Debug.Log("Clicked on resource");
        //If over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        Vector2 mousePosition = InputManager.Instance.mousePos.ReadValue<Vector2>();

        UIManager.Instance.ShowResourceInfoPanel(this, mousePosition);
    }
}