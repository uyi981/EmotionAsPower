using UnityEngine;

public class GameSavedNotification : MonoBehaviour
{
    [SerializeField] private float displayDuration = 2f; // Duration to show notification in seconds

    private void OnEnable()
    {
        // Schedule the disable after displayDuration seconds
        Invoke(nameof(DisableSelf), displayDuration);
    }

    private void DisableSelf()
    {
        gameObject.SetActive(false);
    }
}