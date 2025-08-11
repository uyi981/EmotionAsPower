using UnityEngine;
using System.Collections;

public class ShakeEffect : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private bool shakeOnlyX = true;

    [Header("Auto Setup")]
    [SerializeField] private bool autoConnectToHealth = true;

    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;
    private Health healthComponent;

    private void Awake()
    {
        originalPosition = transform.localPosition;

        healthComponent = GetComponent<Health>();
    }

    private void Start()
    {
        if (autoConnectToHealth && healthComponent != null)
        {
            ConnectToHealth(healthComponent);
        }
    }

    public void ConnectToHealth(Health health)
    {
        if (health != null)
        {
            if (healthComponent != null)
            {
                healthComponent.OnDamageTaken.RemoveListener(OnDamageTaken);
            }

            healthComponent = health;
            healthComponent.OnDamageTaken.AddListener(OnDamageTaken);
        }
    }

    public void DisconnectFromHealth()
    {
        if (healthComponent != null)
        {
            healthComponent.OnDamageTaken.RemoveListener(OnDamageTaken);
            healthComponent = null;
        }
    }

    private void OnDamageTaken(float damageAmount)
    {
        float scaledIntensity = shakeIntensity * Mathf.Clamp01(damageAmount / 50f); // Adjust 50f based on your damage scale
        StartShake(scaledIntensity);
    }

    public void StartShake()
    {
        StartShake(shakeIntensity);
    }

    public void StartShake(float intensity)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity));
    }

    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        transform.localPosition = originalPosition;
    }

    private IEnumerator ShakeCoroutine(float intensity)
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float progress = elapsed / shakeDuration;
            float curveValue = shakeCurve.Evaluate(progress);
            float currentIntensity = intensity * curveValue;

            Vector3 shakeOffset = Vector3.zero;

            if (shakeOnlyX)
            {
                shakeOffset.x = Random.Range(-currentIntensity, currentIntensity);
            }
            else
            {
                shakeOffset = new Vector3(
                    Random.Range(-currentIntensity, currentIntensity),
                    Random.Range(-currentIntensity, currentIntensity),
                    Random.Range(-currentIntensity, currentIntensity)
                );
            }

            transform.localPosition = originalPosition + shakeOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset to original position
        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }

    private void OnDestroy()
    {
        // Clean up
        DisconnectFromHealth();
        StopShake();
    }

    private void OnValidate()
    {
        shakeDuration = Mathf.Max(0f, shakeDuration);
        shakeIntensity = Mathf.Max(0f, shakeIntensity);
    }

    public void SetShakeDuration(float duration)
    {
        shakeDuration = Mathf.Max(0f, duration);
    }

    public void SetShakeIntensity(float intensity)
    {
        shakeIntensity = Mathf.Max(0f, intensity);
    }

    public void SetShakeOnlyX(bool xOnly)
    {
        shakeOnlyX = xOnly;
    }

    public bool IsShaking => shakeCoroutine != null;
}