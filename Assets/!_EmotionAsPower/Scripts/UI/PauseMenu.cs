using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseMenuPanel;
    private int enterDebugModeCondition = 0;

    private void Start()
    {
        pauseMenuPanel.SetActive(false);
    }

    public void ShowPauseMenu()
    {
        GameManager.Instance.PauseGame();
        pauseMenuPanel.SetActive(true);

    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        GameManager.Instance.ResumeGame();
    }

    public void BackToHome()
    {
        UIManager.Instance.BackToHome();
    }

    public void ExitGame()
    {
        GameManager.Instance.ExitGame();
    }

    public void AddPointToEnterDebugMode()
    {
        enterDebugModeCondition++;
        if (enterDebugModeCondition == 5)
        {
            GameManager.Instance.ToggleDebugMode();
            enterDebugModeCondition = 0;
        }
    }

    public void ShowTutorial()
    {
        UIManager.Instance.tutorialPanel.ShowTutorial();
    }
}