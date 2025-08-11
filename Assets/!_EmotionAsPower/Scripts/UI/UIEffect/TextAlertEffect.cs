using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextAlertEffect : MonoBehaviour
{
    [Header("Red Alert Settings")]
    [Tooltip("The base color of the text (e.g., white or default).")]
    public Color baseColor = Color.white;

    [Tooltip("The alert color (e.g., red).")]
    public Color alertColor = Color.red;

    [Tooltip("Speed of the color flashing (higher = faster).")]
    public float colorFlashSpeed = 2f;

    [Header("Breath Scale Settings")]
    [Tooltip("Minimum scale factor for breathing.")]
    public float minScale = 0.9f;

    [Tooltip("Maximum scale factor for breathing.")]
    public float maxScale = 1.1f;

    [Tooltip("Speed of the breathing animation (higher = faster).")]
    public float breathSpeed = 1f;

    [Header("Brightness Settings")]
    [Tooltip("Minimum brightness multiplier.")]
    public float minBrightness = 1f;

    [Tooltip("Maximum brightness multiplier.")]
    public float maxBrightness = 1.5f;

    [Tooltip("Speed of the brightness pulsing (higher = faster).")]
    public float brightnessSpeed = 1.5f;

    private TextMeshProUGUI textComponent;
    private RectTransform rectTransform;
    private Vector3 originalScale;

    void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    void Update()
    {
        // Red Alert Effect: Flash between base and alert color
        float colorLerp = (Mathf.Sin(Time.time * colorFlashSpeed) + 1f) / 2f;
        Color currentColor = Color.Lerp(baseColor, alertColor, colorLerp);

        // Brightness Effect: Pulse brightness by multiplying RGB values
        float brightnessLerp = (Mathf.Sin(Time.time * brightnessSpeed) + 1f) / 2f;
        float brightness = Mathf.Lerp(minBrightness, maxBrightness, brightnessLerp);
        currentColor.r *= brightness;
        currentColor.g *= brightness;
        currentColor.b *= brightness;

        textComponent.color = currentColor;

        // Breath Effect on Scale: Oscillate scale up and down
        float scaleLerp = (Mathf.Sin(Time.time * breathSpeed) + 1f) / 2f;
        float scaleFactor = Mathf.Lerp(minScale, maxScale, scaleLerp);
        rectTransform.localScale = originalScale * scaleFactor;
    }
}