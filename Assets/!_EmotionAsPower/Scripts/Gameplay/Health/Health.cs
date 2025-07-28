using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Health : MonoBehaviour, IInteractable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool canRegenerate = false;
    [SerializeField] private float regenerationRate = 5f; 
    [SerializeField] private float regenerationDelay = 3f; 

    [Header("Damage Settings")]
    [SerializeField] private bool isInvulnerable = false;
    [SerializeField] private float invulnerabilityDuration = 0.5f;
    [SerializeField] private bool canDieMultipleTimes = false;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent<float> OnDamageTaken;
    public UnityEvent<float> OnHealthRecovered;
    public UnityEvent OnDeath;
    public UnityEvent OnRevived;
    public UnityEvent OnMaxHealthReached;

    private bool isDead = false;
    private bool isRegenerating = false;
    private Coroutine regenerationCoroutine;
    private Coroutine invulnerabilityCoroutine;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => currentHealth / maxHealth;
    public bool IsDead => isDead;
    public bool IsInvulnerable => isInvulnerable;
    public bool IsAtMaxHealth => Mathf.Approximately(currentHealth, maxHealth);

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth);
    }

    public bool TakeDamage(float damage, bool ignoreInvulnerability = false)
    {
        if (isDead && !canDieMultipleTimes) return false;
        if (isInvulnerable && !ignoreInvulnerability) return false;
        if (damage <= 0) return false;

        float actualDamage = Mathf.Min(damage, currentHealth);
        currentHealth = Mathf.Max(0, currentHealth - damage);

        OnDamageTaken?.Invoke(actualDamage);
        OnHealthChanged?.Invoke(currentHealth);

        StopRegeneration();

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }

        if (!ignoreInvulnerability && invulnerabilityDuration > 0)
        {
            StartInvulnerability();
        }

        if (canRegenerate && !isDead)
        {
            StartRegenerationWithDelay();
        }

        return true;
    }

    public float RecoverHealth(float healAmount)
    {
        if (isDead || healAmount <= 0) return 0f;

        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        float actualHealing = currentHealth - oldHealth;

        if (actualHealing > 0)
        {
            OnHealthRecovered?.Invoke(actualHealing);
            OnHealthChanged?.Invoke(currentHealth);

            if (IsAtMaxHealth)
            {
                OnMaxHealthReached?.Invoke();
            }
        }

        return actualHealing;
    }

    public void SetHealth(float newHealth)
    {
        float oldHealth = currentHealth;
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);

        if (!Mathf.Approximately(oldHealth, currentHealth))
        {
            OnHealthChanged?.Invoke(currentHealth);

            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
            else if (IsAtMaxHealth)
            {
                OnMaxHealthReached?.Invoke();
            }
        }
    }

    public void SetMaxHealth(float newMaxHealth, bool adjustCurrentHealth = false)
    {
        if (newMaxHealth <= 0) return;

        if (adjustCurrentHealth)
        {
            float healthPercentage = HealthPercentage;
            maxHealth = newMaxHealth;
            currentHealth = maxHealth * healthPercentage;
        }
        else
        {
            maxHealth = newMaxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        OnHealthChanged?.Invoke(currentHealth);
    }

    public void Die()
    {
        if (isDead && !canDieMultipleTimes) return;

        isDead = true;
        currentHealth = 0;
        StopRegeneration();

        OnDeath?.Invoke();
        OnHealthChanged?.Invoke(currentHealth);
    }

   public void Revive(float? reviveHealth = null)
    {
        if (!isDead) return;

        isDead = false;
        currentHealth = reviveHealth ?? maxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 1, maxHealth);

        OnRevived?.Invoke();
        OnHealthChanged?.Invoke(currentHealth);

        if (canRegenerate)
        {
            StartRegenerationWithDelay();
        }
    }

    public void FullHeal()
    {
        RecoverHealth(maxHealth);
    }

    public void StartInvulnerability()
    {
        if (invulnerabilityCoroutine != null)
        {
            StopCoroutine(invulnerabilityCoroutine);
        }
        invulnerabilityCoroutine = StartCoroutine(InvulnerabilityCoroutine());
    }

    public void EndInvulnerability()
    {
        if (invulnerabilityCoroutine != null)
        {
            StopCoroutine(invulnerabilityCoroutine);
            invulnerabilityCoroutine = null;
        }
        isInvulnerable = false;
    }

    private void StartRegenerationWithDelay()
    {
        if (regenerationCoroutine != null)
        {
            StopCoroutine(regenerationCoroutine);
        }
        regenerationCoroutine = StartCoroutine(RegenerationCoroutine());
    }

    private void StopRegeneration()
    {
        if (regenerationCoroutine != null)
        {
            StopCoroutine(regenerationCoroutine);
            regenerationCoroutine = null;
        }
        isRegenerating = false;
    }

    private IEnumerator RegenerationCoroutine()
    {
        isRegenerating = false;

        // Wait for regeneration delay
        yield return new WaitForSeconds(regenerationDelay);

        isRegenerating = true;

        // Regenerate health over time
        while (currentHealth < maxHealth && !isDead)
        {
            float healAmount = regenerationRate * Time.deltaTime;
            RecoverHealth(healAmount);
            yield return null;
        }

        isRegenerating = false;
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
        invulnerabilityCoroutine = null;
    }

    private void OnDestroy()
    {
        StopRegeneration();
        EndInvulnerability();
    }
    protected virtual void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        regenerationRate = Mathf.Max(0, regenerationRate);
        regenerationDelay = Mathf.Max(0, regenerationDelay);
        invulnerabilityDuration = Mathf.Max(0, invulnerabilityDuration);

        if (Application.isPlaying)
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }
    }

    public void OnInteract()
    {
        //throw new System.NotImplementedException();
    }

    public InteractableType GetInteractableType() => InteractableType.Any;
}