using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceInfoPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI typeLabel;
    [SerializeField] private TextMeshProUGUI healthLabel;
    [SerializeField] private TextMeshProUGUI statusLabel;
    [SerializeField] private Button setHarvestBtn;
    [SerializeField] private Button unsetHarvestBtn;
    [SerializeField] private Button cancelBtn;

    [SerializeField]
    [Tooltip("The resource this panel will display information for.")]
    private Resource resource;
    public Resource Resource => resource;  

    private void Awake()
    {
        // Setup button listeners
        if (setHarvestBtn != null)
            setHarvestBtn.onClick.AddListener(SetHarvest);

        if (unsetHarvestBtn != null)
            unsetHarvestBtn.onClick.AddListener(UnsetHarvest);

        if (cancelBtn != null)
            cancelBtn.onClick.AddListener(Hide);
    }

    public void Show(Resource resource)
    {
        if (resource == null) return;

        this.gameObject.SetActive(true);
        this.resource = resource;

        UpdateDisplay();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        this.resource = null;
    }

    private void UpdateDisplay()
    {
        if (resource == null) return;

        // Update basic info
        if (nameLabel != null)
            nameLabel.text = resource.DisplayName;

        if (typeLabel != null)
            typeLabel.text = "Resource";

        // Update health info
        if (healthLabel != null)
        {
            float healthPercent = resource.GetHealthPercentage();
            healthLabel.text = $"Health: {healthPercent:P0}";
        }

        // Update status
        if (statusLabel != null)
        {
            string status = resource.IsDepleted ? "Depleted" :
                           resource.IsForHarvest ? "Marked for Harvest" : "Available";
            statusLabel.text = $"Status: {status}";
        }

        // Update button visibility
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        if (resource == null) return;

        bool isForHarvest = resource.IsForHarvest;
        bool isDepleted = resource.IsDepleted;

        // Show/hide buttons based on resource state
        if (setHarvestBtn != null)
            setHarvestBtn.gameObject.SetActive(!isForHarvest);

        if (unsetHarvestBtn != null)
            unsetHarvestBtn.gameObject.SetActive(isForHarvest);
    }

    public void SetHarvest()
    {
        if (resource != null && !resource.IsDepleted)
        {
            resource.SetForHarvest();
            UpdateDisplay();
        }
    }

    public void UnsetHarvest()
    {
        if (resource != null)
        {
            resource.UnsetForHarvest();
            UpdateDisplay();
        }
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (setHarvestBtn != null)
            setHarvestBtn.onClick.RemoveListener(SetHarvest);

        if (unsetHarvestBtn != null)
            unsetHarvestBtn.onClick.RemoveListener(UnsetHarvest);

        if (cancelBtn != null)
            cancelBtn.onClick.RemoveListener(Hide);
    }
}