using Assets.__EmotionAsPower.Scripts.Gameplay.Building.BuidingChild;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>, ISetup
{

    [SerializeField]
    private bool showUI = false;

    [Header("UI Panels")]
    public GameObject loseGamePanel;
    public GameObject winGamePanel;
    public ResourceInfoPanel resourceInfoPanel;
    public ItemInfoPanel itemInfoPanel;
    public TutorialContainer tutorialPanel;

    [Header("Settings")]
    public string homeScreenSceneName = "HomeScreen";

    [Header("Notifications")]
    public GameSavedNotification savedNotification;

    [Header("Breeding")]
    public BreedingUI breedingUI;

    public bool ShowUI => showUI;

    protected override void Awake()
    {
        base.Awake();
        if(resourceInfoPanel == null)
        {
            resourceInfoPanel = GetComponentInChildren<ResourceInfoPanel>();
        }
        if(itemInfoPanel == null)
        {
            itemInfoPanel = GetComponentInChildren<ItemInfoPanel>();
        }
        if(tutorialPanel == null)
        {
            tutorialPanel = GetComponentInChildren<TutorialContainer>();
        }
        if (winGamePanel != null)
        {
            winGamePanel.SetActive(false);
        }
    }
    private void Start()
    {
        
        GameManager.Instance.OnSetupFinished += NewGameInit;
    }
    // Update your Setup() method to include:
    public void Setup()
    {
        showUI = true;
        if (savedNotification != null)
        {
            DataPersistenceManager.Instance.OnGameSaved +=
                () => savedNotification.gameObject.SetActive(true);
        }

        // Hide info panels at start
        if (resourceInfoPanel != null)
        {
            resourceInfoPanel.Hide();
        }

        if (itemInfoPanel != null)
        {
            itemInfoPanel.Hide();
        }
    }

    public void ToggleLoseGamePanel(bool show)
    {
        loseGamePanel.SetActive(show);
    }

    public void BackToHome()
    {
        SceneManager.LoadSceneAsync(homeScreenSceneName);
    }

    public void OpenBreedingUI(BreedingBuilding breedingBuilding)
    {
        breedingUI.gameObject.SetActive(true);
        breedingUI.SetBreedingBuilding(breedingBuilding);
    }

    public void ShowResourceInfoPanel(Resource resource, Vector3 screenPosition)
    {
        
        if (resourceInfoPanel == null) return;

        // Hide any existing panels first
        HideResourceInfoPanel();
        HideItemInfoPanel();

        // Position the panel at the mouse position
        RectTransform panelRect = resourceInfoPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            // Convert screen position to UI position
            Vector2 uiPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                panelRect.parent as RectTransform,
                screenPosition,
                null,
                out uiPosition
            );

            panelRect.localPosition = uiPosition;

            // Ensure panel stays within screen bounds
            ClampPanelToScreen(panelRect);
        }

        // Show the panel with resource data
        resourceInfoPanel.Show(resource);
    }

    public void HideResourceInfoPanel()
    {
        if (resourceInfoPanel != null)
        {
            resourceInfoPanel.Hide();
        }
    }

    private void ClampPanelToScreen(RectTransform panelRect)
    {
        Vector3[] corners = new Vector3[4];
        panelRect.GetWorldCorners(corners);

        RectTransform canvasRect = panelRect.root as RectTransform;
        if (canvasRect == null) return;

        Vector3[] canvasCorners = new Vector3[4];
        canvasRect.GetWorldCorners(canvasCorners);

        Vector3 position = panelRect.position;

        // Get panel dimensions
        float panelWidth = corners[2].x - corners[0].x;
        float panelHeight = corners[2].y - corners[0].y;

        // Clamp to screen bounds
        if (corners[2].x > canvasCorners[2].x) // Right edge
            position.x -= (corners[2].x - canvasCorners[2].x);
        if (corners[0].x < canvasCorners[0].x) // Left edge
            position.x += (canvasCorners[0].x - corners[0].x);
        if (corners[2].y > canvasCorners[2].y) // Top edge
            position.y -= (corners[2].y - canvasCorners[2].y);
        if (corners[0].y < canvasCorners[0].y) // Bottom edge
            position.y += (canvasCorners[0].y - corners[0].y);

        panelRect.position = position;
    }
    public void ShowItemInfoPanel(Item item, Vector3 screenPosition)
    {
        if (itemInfoPanel == null) return;

        // Hide any existing panels first
        HideResourceInfoPanel();
        HideItemInfoPanel();

        // Position the panel at the mouse position
        RectTransform panelRect = itemInfoPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            // Convert screen position to UI position
            Vector2 uiPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                panelRect.parent as RectTransform,
                screenPosition,
                null,
                out uiPosition
            );
            panelRect.localPosition = uiPosition;

            // Ensure panel stays within screen bounds
            ClampPanelToScreen(panelRect);
        }

        // Show the panel with item data
        itemInfoPanel.Show(item.ItemSO);
    }

    public void ShowItemInfoPanel(ItemSO itemSO, Vector3 screenPosition)
    {
        if (itemInfoPanel == null) return;

        // Hide any existing panels first
        HideResourceInfoPanel();
        HideItemInfoPanel();

        // Position the panel at the mouse position
        RectTransform panelRect = itemInfoPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            // Convert screen position to UI position
            Vector2 uiPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                panelRect.parent as RectTransform,
                screenPosition,
                null,
                out uiPosition
            );
            panelRect.localPosition = uiPosition;

            // Ensure panel stays within screen bounds
            ClampPanelToScreen(panelRect);
        }

        // Show the panel with item data
        itemInfoPanel.Show(itemSO);
    }

    public void HideItemInfoPanel()
    {
        if (itemInfoPanel != null)
        {
            itemInfoPanel.Hide();
        }
    }

    public void NewGameInit()
    {
        //NOt load = new game
        if (!DataPersistenceManager.Instance.gameDataView.shouldLoad)
        {
            tutorialPanel.ShowTutorial();
        }
        else
        {
            tutorialPanel.SkipTutorial();
        }
    }

    public void CheckIfWinGame(StageOfDay stageOfDay) { 
        if(stageOfDay.day == 20)
        {
            WinGame();
        }
    }

    public void WinGame()
    {
        GameManager.Instance.PauseGame();
        if (winGamePanel != null) { 
            winGamePanel.SetActive(true);
        }
    }
}