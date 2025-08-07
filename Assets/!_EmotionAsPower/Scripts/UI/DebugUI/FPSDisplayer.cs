using TMPro;
using UnityEngine;

public class FPSDisplayer : MonoBehaviour
{
    private TextMeshProUGUI label;

    private void Start()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        SetText();
    }

    private void SetText()
    {
        if (GameManager.Instance != null)
        {
            float fps = GameManager.Instance.CurrentFPS;
            label.text = $"{fps:F0} FPS";
        }
    }
}