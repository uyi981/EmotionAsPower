using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(ItemDropper))]
public class Resource : MonoBehaviour, IInteractable
{
    [Header("Resource Properties")]
    [SerializeField]
    private ResourceSO resourceSO;
    public ResourceSO ResourceSO => resourceSO;

    [SerializeField]
    private string displayName;
    public string DisplayName => !string.IsNullOrEmpty(displayName) ? displayName : (resourceSO != null ? resourceSO.DisplayName : "Unknown Resource");

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
    private TextMeshProUGUI healthText;
    private bool isInitialized = false;
    private float lastHarvestTime = 0f;

    // Properties
    public Health Health => health;
    public ItemDropper ItemDropper => itemDropper;
    public bool CanHarvest => canBeHarvested && !health.IsDead && Time.time >= lastHarvestTime + harvestCooldown;
    public bool IsDepleted => health != null && health.IsDead;

    private void Awake()
    {
        // Get or add required components
        health = GetComponent<Health>();

        itemDropper = GetComponent<ItemDropper>();

        // Find health text component
        healthText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (resourceSO != null && !isInitialized)
        {
            Initialize(resourceSO);
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnHealthChanged.AddListener(OnHealthChanged);
            health.OnDeath.AddListener(OnResourceDepletedInternal);
            health.OnRevived.AddListener(OnResourceRespawnedInternal);
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChanged.RemoveListener(OnHealthChanged);
            health.OnDeath.RemoveListener(OnResourceDepletedInternal);
            health.OnRevived.RemoveListener(OnResourceRespawnedInternal);
        }
    }

    public void Initialize(ResourceSO resourceSO)
    {
        if (resourceSO == null)
        {
            Debug.LogWarning("Cannot initialize Resource with null ResourceSO!");
            return;
        }

        this.resourceSO = resourceSO;
        this.gameObject.name = resourceSO.DisplayName;

        // Initialize health component
        if (health != null)
        {
            health.SetMaxHealth(resourceSO.maxHealth, true);
            health.SetHealth(resourceSO.maxHealth);
        }

        // Initialize item dropper
        if (itemDropper != null)
        {
            itemDropper.Initialize(resourceSO);
        }

        // Set material
        SetMaterial();

        // Update UI
        UpdateHealthUI();

        isInitialized = true;
        Debug.Log($"Resource '{DisplayName}' initialized successfully");
    }

    public void OnInteract()
    {
        if (!CanHarvest)
        {
            if (health.IsDead)
            {
                Debug.Log($"Resource '{DisplayName}' is depleted and cannot be harvested.");
            }
            else if (Time.time < lastHarvestTime + harvestCooldown)
            {
                Debug.Log($"Resource '{DisplayName}' is on cooldown. Wait {(lastHarvestTime + harvestCooldown - Time.time):F1} seconds.");
            }
            return;
        }

        HarvestResource();
    }

    public InteractableType GetInteractableType() => InteractableType.Resource;

    public void HarvestResource(float damageAmount = 1f)
    {
        if (!CanHarvest) return;

        // Use resource's harvest damage or default
        float damage = damageAmount > 0 ? damageAmount : (resourceSO != null ? resourceSO.harvestDamage : 10f);

        // Deal damage to the resource
        bool damageTaken = health.TakeDamage(damage);

        if (damageTaken)
        {
            lastHarvestTime = Time.time;

            // Drop items
            if (itemDropper != null)
            {
                itemDropper.DropFunction(transform.position, false);
            }

            OnResourceHarvested?.Invoke();
            Debug.Log($"Harvested '{DisplayName}' - Health: {health.CurrentHealth}/{health.MaxHealth}");
        }
    }

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

    private void SetMaterial()
    {
        if (resourceSO == null || resourceSO.Icon == null) return;

        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.SetTexture(textureProperty, resourceSO.Icon.texture);
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null && showHealthUI)
        {
            if (health != null)
            {
                healthText.text = $"{health.CurrentHealth:F0}/{health.MaxHealth:F0}";
                healthText.gameObject.SetActive(!health.IsDead);
            }
        }
        else if (healthText != null && !showHealthUI)
        {
            healthText.gameObject.SetActive(false);
        }
    }

    private void OnHealthChanged(float currentHealth)
    {
        UpdateHealthUI();
    }

    private void OnResourceDepletedInternal()
    {
        Debug.Log($"Resource '{DisplayName}' has been depleted!");

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
        Debug.Log($"Resource '{DisplayName}' has respawned!");
        UpdateHealthUI();
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