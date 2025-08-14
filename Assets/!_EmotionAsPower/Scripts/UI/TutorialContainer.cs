using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class TutorialContainer : MonoBehaviour
{
    public GameObject tutorialPanel;
    [Header("Tutorial Configuration")]
    public TutorialPanelHolder[] TutorialPanels;

    [Header("UI References")]
    public Button nextButton;
    public Button skipButton;
    public TextMeshProUGUI nextButtonText;
    public UIPanelSlider panelSlider;

    [Header("Button Text Settings")]
    public string nextButtonDefaultText = "Next";
    public string nextButtonFinishText = "Finish";

    [Header("Events")]
    public UnityEvent onTutorialFinished;
    public UnityEvent onTutorialStarted;

    private int currentTutorialIndex = -1;
    private bool isTutorialActive = false;

    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(() => SkipTutorial());
        }
    }

    public void ShowTutorial()
    {
        ShowTutorial(0);
    }

    public void ShowTutorial(int index)
    {
        if (index < 0 || index >= TutorialPanels.Length)
        {
            Debug.LogWarning($"Tutorial index {index} is out of range. Valid range: 0-{TutorialPanels.Length - 1}");
            return;
        }

        // Only pause if we're starting a new tutorial session
        if (!isTutorialActive && GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
            onTutorialStarted?.Invoke();
        }

        isTutorialActive = true;

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }

        if (panelSlider != null)
        {
            panelSlider.ShowPanel();
        }

        HideAllTutorials();

        var holder = TutorialPanels[index];
        if (holder.tutorialPanel != null)
        {
            holder.tutorialPanel.gameObject.SetActive(true);
            currentTutorialIndex = index;
            holder.onTutorialShow?.Invoke();
            UpdateNextButtonText();
        }
    }

    private void OnNextButtonClicked()
    {
        // Prevent multiple clicks
        if (nextButton != null)
        {
            nextButton.interactable = false;
        }

        bool isLastTutorial = currentTutorialIndex == TutorialPanels.Length - 1;

        if (isLastTutorial)
        {
            // Add a small delay before finishing to ensure UI interactions complete
            StartCoroutine(FinishTutorialDelayed());
        }
        else
        {
            ShowNextTutorial();
            // Re-enable button after showing next tutorial
            if (nextButton != null)
            {
                nextButton.interactable = true;
            }
        }
    }

    private System.Collections.IEnumerator FinishTutorialDelayed()
    {
        yield return new WaitForEndOfFrame();
        FinishTutorial();
    }

    public void ShowNextTutorial()
    {
        int nextIndex = currentTutorialIndex + 1;
        if (nextIndex < TutorialPanels.Length)
        {
            ShowTutorial(nextIndex);
        }
        else
        {
            FinishTutorial();
        }
    }

    public void ShowPreviousTutorial()
    {
        int previousIndex = currentTutorialIndex - 1;
        if (previousIndex >= 0)
        {
            ShowTutorial(previousIndex);
        }
    }

    public void SkipTutorial()
    {
        FinishTutorial();
    }

    public void ReplayTutorial()
    {
        if (currentTutorialIndex >= 0 && currentTutorialIndex < TutorialPanels.Length)
        {
            ShowTutorial(currentTutorialIndex);
        }
    }

    public void FinishTutorial()
    {
        // Invoke finish event before cleaning up
        onTutorialFinished?.Invoke();

        // Hide all tutorial panels
        HideAllTutorials();

        // Hide the panel slider
        if (panelSlider != null)
        {
            panelSlider.HidePanel();
        }

        // Hide the main tutorial panel
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // Reset state
        currentTutorialIndex = -1;
        isTutorialActive = false;

        // Re-enable buttons to ensure they work for next time
        EnableButtons();

        // Resume game safely
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
        else
        {
            Debug.LogWarning("GameManager instance is null. Cannot resume game.");
        }
    }

    private void EnableButtons()
    {
        if (nextButton != null)
        {
            nextButton.interactable = true;
        }
        if (skipButton != null)
        {
            skipButton.interactable = true;
        }
    }

    private void HideAllTutorials()
    {
        foreach (var holder in TutorialPanels)
        {
            if (holder.tutorialPanel != null)
            {
                holder.tutorialPanel.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateNextButtonText()
    {
        if (nextButtonText == null) return;

        bool isLastTutorial = currentTutorialIndex == TutorialPanels.Length - 1;
        nextButtonText.text = isLastTutorial ? nextButtonFinishText : nextButtonDefaultText;

        // Ensure button remains interactable when updating text
        if (nextButton != null)
        {
            nextButton.interactable = true;
        }
    }

    public int GetCurrentTutorialIndex()
    {
        return currentTutorialIndex;
    }

    public bool IsTutorialActive()
    {
        return isTutorialActive;
    }

    public int GetTotalTutorialCount()
    {
        return TutorialPanels.Length;
    }

    public void CallPause()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
    }
}

    [Serializable]
public class TutorialPanelHolder
{
    [Header("Panel Reference")]
    public RectTransform tutorialPanel;

    [Header("Events")]
    [Tooltip("Event invoked when this tutorial panel is shown")]
    public UnityEvent onTutorialShow;
}