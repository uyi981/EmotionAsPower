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
    public TextMeshProUGUI nextButtonText;
    public UIPanelSlider panelSlider;

    [Header("Button Text Settings")]
    public string nextButtonDefaultText = "Next";
    public string nextButtonFinishText = "Finish";

    private int currentTutorialIndex = -1;

    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
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

        GameManager.Instance.PauseGame();

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
        bool isLastTutorial = currentTutorialIndex == TutorialPanels.Length - 1;

        if (isLastTutorial)
        {
            FinishTutorial();
        }
        else
        {
            ShowNextTutorial();
        }
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
        HideAllTutorials();

        if (panelSlider != null)
        {
            panelSlider.HidePanel();
        }

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        currentTutorialIndex = -1;
        GameManager.Instance.ResumeGame();
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
    }

    public int GetCurrentTutorialIndex()
    {
        return currentTutorialIndex;
    }

    public bool IsTutorialActive()
    {
        return currentTutorialIndex >= 0;
    }

    public int GetTotalTutorialCount()
    {
        return TutorialPanels.Length;
    }

    public void CallPause()
    {
        GameManager.Instance.PauseGame();
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