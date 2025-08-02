using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimePanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI timeText;
    [SerializeField]
    private Button pauseButton;
    [SerializeField]
    private Button button1x;
    [SerializeField]
    private Button button2x;
    [SerializeField]
    private Button button4x;

    [SerializeField]
    private Color defaultColor = new Color(1, 1, 1, 0.5f); 
    [SerializeField]
    private Color highlightColor = new Color(0.5f, 0.5f, 0.5f, 1f); 

    private void Start()
    {
        GameManager.Instance.OnSetupFinished += Initialize;
        DayTimeController.Instance.OnStageOfDayChanged += SetText;
    }

    public void Initialize()
    {
        pauseButton.onClick.AddListener(() => SetTimeSpeed(0));
        button1x.onClick.AddListener(() => SetTimeSpeed(1));
        button2x.onClick.AddListener(() => SetTimeSpeed(2));
        button4x.onClick.AddListener(() => SetTimeSpeed(4));

        GameManager.Instance.OnGameSpeedChanged += OnGameSpeedChanged;
        GameManager.Instance.OnGamePaused += OnGamePausedChanged;

        UpdateButtonVisuals();
        GameManager.Instance.ResumeGame();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameSpeedChanged -= OnGameSpeedChanged;
            GameManager.Instance.OnGamePaused -= OnGamePausedChanged;
        }
    }

    private void OnGameSpeedChanged(float speed)
    {
        UpdateButtonVisuals();
    }

    private void OnGamePausedChanged(bool isPaused)
    {
        UpdateButtonVisuals();
    }

    public void SetTimeSpeed(int speed)
    {
        if (speed <= 0)
        {
            GameManager.Instance.PauseGame();
        }
        else
        {
            GameManager.Instance.ResumeGame();
            GameManager.Instance.SetGameSpeed(speed);
        }
    }

    private void UpdateButtonVisuals()
    {
        // Reset all buttons to default color
        SetButtonColor(pauseButton, defaultColor);
        SetButtonColor(button1x, defaultColor);
        SetButtonColor(button2x, defaultColor);
        SetButtonColor(button4x, defaultColor);

        // Highlight the button that matches current game state
        if (GameManager.Instance.IsPaused)
        {
            SetButtonColor(pauseButton, highlightColor);
        }
        else
        {
            float currentSpeed = GameManager.Instance.CurrentGameSpeed;
            if (Mathf.Approximately(currentSpeed, 1f))
            {
                SetButtonColor(button1x, highlightColor);
            }
            else if (Mathf.Approximately(currentSpeed, 2f))
            {
                SetButtonColor(button2x, highlightColor);
            }
            else if (Mathf.Approximately(currentSpeed, 4f))
            {
                SetButtonColor(button4x, highlightColor);
            }
        }
    }

    private void SetButtonColor(Button button, Color color)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        button.colors = colors;
    }

    private void SetText(StageOfDay stageOfDay)
    {
        timeText.text = $"Day {stageOfDay.day,3}: {stageOfDay.stage,-10}";
    }
}