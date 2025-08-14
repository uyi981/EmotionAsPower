using TMPro;
using UnityEngine;

public class EnemyWaveNotification : MonoBehaviour
{
    [SerializeField] private GameObject notificationObject; 
    [SerializeField] private float warningTimeSeconds = 30f;

    private DayTimeController dayTimeController;
    private EnemySpawningConfig spawningConfig;
    private bool isShowing = false;
    private StageOfDay nextStageOfDay;
    public TextMeshProUGUI text;
    string notificationText = "Enemies will spawn soon!";
    string notificationEndText = "Enemies Wave is end!";
    private void Awake()
    {
        dayTimeController = DayTimeController.Instance;
        EnemyManager enemyManager = EnemyManager.Instance;
        if (enemyManager != null)
        {
            spawningConfig = enemyManager.SpawningConfig;
        }
        else
        {
            Debug.LogError("EnemyManager not found in the scene!");
        }
    }

    private void Start()
    {
        if (dayTimeController != null)
        {
            dayTimeController.OnStageOfDayChanged += OnStageChanged;
        }
    }

    private void OnDestroy()
    {
        if (dayTimeController != null)
        {
            dayTimeController.OnStageOfDayChanged -= OnStageChanged;
        }
    }

    private void Update()
    {
        if (isShowing || spawningConfig == null) return;

        float remainingSeconds = CalculateRemainingSecondsToNextStage();
        nextStageOfDay = CalculateNextStageOfDay();

        if (spawningConfig.waves.ContainsKey(nextStageOfDay) && remainingSeconds <= warningTimeSeconds)
        {
            if(spawningConfig.waves[nextStageOfDay] != null)
            {
                if (spawningConfig.waves[nextStageOfDay].isEndWave)
                {
                    text.text = notificationEndText;
                }
                else
                {
                    text.text = notificationText;
                }
            }
            ShowNotification();
        }
    }

    private void ShowNotification()
    {
        if (notificationObject != null)
        {
            notificationObject.SetActive(true);
        }
        isShowing = true;
    }

    private void OnStageChanged(StageOfDay newStage)
    {
        if (isShowing)
        {
            if (notificationObject != null)
            {
                notificationObject.SetActive(false);
            }
            isShowing = false;
        }
    }

    private float CalculateRemainingSecondsToNextStage()
    {
        float currentTimeOfDay = dayTimeController.GetTimePercent();
        float[] stageStarts = { 0.25f, 0.5f, 0.75f, 0.875f }; // Morning, Noon, Evening, Night

        float nextStart = float.MaxValue;
        foreach (float start in stageStarts)
        {
            if (start > currentTimeOfDay)
            {
                nextStart = Mathf.Min(nextStart, start);
            }
        }

        if (nextStart == float.MaxValue)
        {
            nextStart = stageStarts[0] + 1f; // Next Morning on the following day
        }

        float remainingFraction = nextStart - currentTimeOfDay;
        float dayDurationSeconds = dayTimeController.dayDurationInMinutes * 60f;
        return remainingFraction * dayDurationSeconds;
    }

    private StageOfDay CalculateNextStageOfDay()
    {
        var currentDateTime = dayTimeController.GetCurrentDateTime();
        DayTimeController.TimeStage currentStage = currentDateTime.timeStage;
        int currentDay = currentDateTime.day;

        DayTimeController.TimeStage nextStage;
        switch (currentStage)
        {
            case DayTimeController.TimeStage.Morning:
                nextStage = DayTimeController.TimeStage.Noon;
                break;
            case DayTimeController.TimeStage.Noon:
                nextStage = DayTimeController.TimeStage.Evening;
                break;
            case DayTimeController.TimeStage.Evening:
                nextStage = DayTimeController.TimeStage.Night;
                break;
            case DayTimeController.TimeStage.Night:
                nextStage = DayTimeController.TimeStage.Morning;
                break;
            default:
                nextStage = DayTimeController.TimeStage.Morning;
                break;
        }

        int nextDay = (currentStage == DayTimeController.TimeStage.Night) ? currentDay + 1 : currentDay;
        return new StageOfDay(nextStage, nextDay);
    }
}