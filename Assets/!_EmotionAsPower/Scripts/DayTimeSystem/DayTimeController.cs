//#if UNITY_EDITOR
//using UnityEditor;
//#endif
using UnityEngine;
using System;
using static DayTimeController;

public class DayTimeController : Singleton<DayTimeController>, IDataPersistence
{
    [Header("Time Settings")]
    [Tooltip("Duration of a full day in real-time minutes. 10f => 10mins in real time")]
    public float dayDurationInMinutes = 10f;

    [SerializeField] private int day = 0;
    [Range(0, 1)]
    [SerializeField] private float timeOfDay = 0f;

    //[Range(0f, 24f)]
    //public float editorTime = 12f; // chỉnh ở editor: 0h–24h

    [Header("Sun + need assign")]
    public Light sunLight;
    public Light moon;
    public Gradient lightColorOverDay;
    public Gradient SkyOverDay;
    public Gradient GroundOverDay;
    public AnimationCurve lightIntensityCurve;

    [Header("Skybox Tint")]
    public Material skyboxMaterial;
    public Material nightSkybox;
    public Gradient skyboxColorOverDay;
    [Header("Light Preset")]
    public Color nightSkyColor;
    public Color daySkyColor;
    public Color nightEquatorColor;
    public Color dayEquatorColor;
    public enum TimeStage { Dawn, Morning, Noon, Evening, Night }
    public TimeStage currentStage;
    private TimeStage previousStage;
    private int previousDay;
    public Action<TimeStage> OnTimeStageChanged;
    public Action<StageOfDay> OnStageOfDayChanged;

    private TimeStage lastStage;
    private HourMinute HourMinute = new HourMinute(0, 0);

    private void Update()
    {
        if (Application.isPlaying)
        {
            float delta = Time.deltaTime / (dayDurationInMinutes * 60f);
            timeOfDay = (timeOfDay + delta) % 1f;
            UpdateLighting();
            CheckTimeStage();

        }
    }
    private void Start()
    {
        //timeOfDay = 0.3f;
    }

    //private void OnValidate()
    //{
    //    if (!Application.isPlaying)
    //    {
    //        timeOfDay = editorTime / 24f;
    //        UpdateLighting();
    //    }
    //}
    void UpdateLighting()
    {
        float sunAngle = timeOfDay * 360f - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
        sunLight.color = lightColorOverDay.Evaluate(timeOfDay);
        sunLight.intensity = lightIntensityCurve.Evaluate(timeOfDay);

        moon.transform.rotation = Quaternion.Euler(sunAngle + 180f, 170f, 0f);
        moon.intensity = lightIntensityCurve.Evaluate(timeOfDay) * 0.5f;
        GetCurrentTime();
        if (HourMinute.hour >=6&&HourMinute.hour<=17)
        {
            RenderSettings.skybox = skyboxMaterial;
            moon.gameObject.SetActive(false);
            sunLight.gameObject.SetActive(true);
        }
        else
        {
            RenderSettings.skybox = nightSkybox;
            moon.gameObject.SetActive(true);
            sunLight.gameObject.SetActive(false);
        }

        if (skyboxMaterial != null)
        {
            skyboxMaterial.SetColor("_Tint", skyboxColorOverDay.Evaluate(timeOfDay));
        }
        //if(timeOfDay>=0.5)
        //{
        //    RenderSettings.ambientSkyColor = Color.Lerp(daySkyColor, nightSkyColor,(timeOfDay-0.5f)*2);
        //    RenderSettings.ambientEquatorColor = Color.Lerp(dayEquatorColor, nightEquatorColor, timeOfDay);
        //}
        //else
        //{
        //    RenderSettings.ambientSkyColor = Color.Lerp(nightSkyColor, daySkyColor, timeOfDay*2);
        //    RenderSettings.ambientEquatorColor = Color.Lerp(nightEquatorColor, dayEquatorColor, timeOfDay);
        //}
            RenderSettings.ambientSkyColor = SkyOverDay.Evaluate(timeOfDay);
            RenderSettings.ambientGroundColor = GroundOverDay.Evaluate(timeOfDay);


    }

    public void GetCurrentTime()
    {
        float value = Remap(timeOfDay, 0, 1, 0, 24);
        int hours = (int)value;
        int minutes = (int)((value - hours) * 60);
        HourMinute.hour = hours;
        HourMinute.minute = minutes;
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    void CheckTimeStage()
    {
        previousStage = currentStage;
        previousDay = day;
        TimeStage newStage = GetTimeStage();
        if (newStage != lastStage)
        {
            currentStage = newStage;
            lastStage = newStage;
            UpdateDay();
            OnTimeStageChanged?.Invoke(newStage);
            OnStageOfDayChanged?.Invoke(new StageOfDay(GetCurrentDateTime()));
            // Save game if day changed 
            if (previousDay != day) {
                DataPersistenceManager.Instance.SaveGame();
            }
        }
    }

    TimeStage GetTimeStage()
    {
        GetCurrentTime();
        if (HourMinute.hour >= 6 && HourMinute.hour < 12) return TimeStage.Morning;
        if (HourMinute.hour >= 12 && HourMinute.hour < 18) return TimeStage.Noon;
        if (HourMinute.hour >= 18 && HourMinute.hour < 21) return TimeStage.Evening;
        if (HourMinute.hour >= 21 || HourMinute.hour < 6) return TimeStage.Night;
        return TimeStage.Morning; // Default to Dawn if no other condition matches
    }

    private void UpdateDay()
    {
        if (currentStage == TimeStage.Morning && previousStage == TimeStage.Night) {
            day++;
            
        }

    }

    public float GetTimePercent() => timeOfDay;

    public GameDateTime GetCurrentDateTime()
    {
        return new GameDateTime
        {
            timeOfDay = timeOfDay,
            timeStage = currentStage,
            day = day,
        };
    }

    public void LoadGame(GameData gameData)
    {
        SetDateTime(gameData.dateTime);
        OnStageOfDayChanged?.Invoke(new StageOfDay(GetCurrentDateTime()));
    }

    public void SaveGame(ref GameData gameData)
    {
        gameData.dateTime = GetCurrentDateTime();
    }

    public void SetDateTime(GameDateTime dateTime)
    {
        this.timeOfDay = dateTime.timeOfDay;
        this.currentStage = dateTime.timeStage;
        this.day = dateTime.day;
    }
}

[Serializable]
public struct GameDateTime
{
    public float timeOfDay;
    public TimeStage timeStage;
    public int day;
}

[Serializable]
public class StageOfDay
{
    public TimeStage stage;
    public int day;

    public StageOfDay(TimeStage stage, int day)
    {
        this.stage = stage;
        this.day = day;
    }

    public StageOfDay(GameDateTime dateTime)
    {
        this.stage = dateTime.timeStage;
        this.day = dateTime.day;
    }

    public bool Match(GameDateTime dateTime)
    {
        return stage == dateTime.timeStage && day == dateTime.day;
    }

    public override bool Equals(object obj)
    {
        return obj is StageOfDay condition &&
               stage == condition.stage &&
               day == condition.day;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(stage, day);
    }
}

public struct HourMinute
{
    public int hour;
    public int minute;

    public HourMinute(int h, int m)
    {
        hour = h;
        minute = m;
    }

    public override string ToString()
    {
        return $"{hour:D2}:{minute:D2}";
    }
}
public enum TypeOfTime
{
    noon,
    midnight,
    dawn,
    dusk,
    morning,
    afternoon,
}
