using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(ItemDropper))]
[RequireComponent(typeof(ShakeEffect))]
public class Resource : MonoBehaviour, IInteractable
{
    [Header("Resource Properties")]
    [SerializeField]
    private string id;
    public string ID => id;
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

    [Header("Events")]
    public UnityEvent OnResourceHarvested;
    public UnityEvent OnResourceDepleted;
    public UnityEvent OnResourceRespawned;

    // Components
    private Health health;
    private ItemDropper itemDropper;
    private ShakeEffect shakeEffect;
    private TextMeshProUGUI healthText;
    private bool isInitialized = false;
    private float lastHarvestTime = 0f;

    // Properties
    public Health Health => health;
    public ItemDropper ItemDropper => itemDropper;
    public ShakeEffect ShakeEffect => shakeEffect;
    public bool CanHarvest => canBeHarvested && !health.IsDead && Time.time >= lastHarvestTime + harvestCooldown;
    public bool IsDepleted => health != null && health.IsDead;

    private void Awake()
    {
        // Get or add required components
        health = GetComponent<Health>();
        itemDropper = GetComponent<ItemDropper>();
        shakeEffect = GetComponent<ShakeEffect>();

        // Find health text component
        healthText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (!isInitialized)
        {
            Initialize();
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
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

        isInitialized = true;
    }

    public void OnInteract()
    {
        if (!CanHarvest)
        {
            if (health.IsDead)
            {
                //Debug.Log($"Resource '{DisplayName}' is depleted and cannot be harvested.");
            }
            else if (Time.time < lastHarvestTime + harvestCooldown)
            {
                //Debug.Log($"Resource '{DisplayName}' is on cooldown. Wait {(lastHarvestTime + harvestCooldown - Time.time):F1} seconds.");
            }
            return;
        }

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

    // Method to manually trigger shake (useful for testing or special effects)
    public void TriggerShake()
    {
        if (shakeEffect != null)
        {
            shakeEffect.StartShake();
        }
    }

    private void OnResourceDepletedInternal()
    {
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
        OnResourceRespawned?.Invoke();
    }

    private void DestroyResource()
    {
        Destroy(gameObject);
    }

    // Utility methods
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
    }
}