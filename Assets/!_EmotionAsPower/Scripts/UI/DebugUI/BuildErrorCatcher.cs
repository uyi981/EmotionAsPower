using UnityEngine;
using TMPro;
using System.Text;

public class BuildErrorCatcher : MonoBehaviour
{
    [Header("Display Settings")]
    public TextMeshProUGUI errorText;
    public LogFilter logFilter = LogFilter.All;
    public int maxLines = 10;

    [Header("Colors")]
    public Color errorColor = Color.red;
    public Color warningColor = Color.yellow;
    public Color logColor = Color.white;

    private StringBuilder logBuilder = new StringBuilder();
    private int currentLineCount = 0;

    public enum LogFilter
    {
        All,
        ErrorOnly,
        WarningOnly,
        LogOnly
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void Start()
    {
        if (errorText == null)
        {
            Debug.LogWarning("ErrorText TextMeshPro component not assigned!");
            return;
        }

        errorText.text = "Build Error Catcher Ready...";
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Filter based on enum selection
        if (!ShouldDisplayLog(type))
            return;

        // Get color based on log type
        Color textColor = GetLogColor(type);
        string colorHex = ColorUtility.ToHtmlStringRGB(textColor);

        // Format the message
        string formattedMessage = $"<color=#{colorHex}>[{type}] {logString}</color>";

        // Add to log builder
        logBuilder.AppendLine(formattedMessage);
        currentLineCount++;

        // Remove old lines if exceeding max
        if (currentLineCount > maxLines)
        {
            RemoveOldestLine();
        }

        // Update UI
        UpdateErrorDisplay();
    }

    private bool ShouldDisplayLog(LogType type)
    {
        switch (logFilter)
        {
            case LogFilter.ErrorOnly:
                return type == LogType.Error || type == LogType.Exception;
            case LogFilter.WarningOnly:
                return type == LogType.Warning;
            case LogFilter.LogOnly:
                return type == LogType.Log;
            case LogFilter.All:
            default:
                return true;
        }
    }

    private Color GetLogColor(LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                return errorColor;
            case LogType.Warning:
                return warningColor;
            case LogType.Log:
            default:
                return logColor;
        }
    }

    private void RemoveOldestLine()
    {
        string logText = logBuilder.ToString();
        int firstNewLine = logText.IndexOf('\n');
        if (firstNewLine >= 0)
        {
            logBuilder.Remove(0, firstNewLine + 1);
            currentLineCount--;
        }
    }

    private void UpdateErrorDisplay()
    {
        if (errorText != null)
        {
            errorText.text = logBuilder.ToString();
        }
    }

    [ContextMenu("Clear Logs")]
    public void ClearLogs()
    {
        logBuilder.Clear();
        currentLineCount = 0;
        if (errorText != null)
        {
            errorText.text = "Logs cleared...";
        }
    }

    [ContextMenu("Test Error")]
    public void TestError()
    {
        Debug.LogError("Test error message");
    }

    [ContextMenu("Test Warning")]
    public void TestWarning()
    {
        Debug.LogWarning("Test warning message");
    }

    [ContextMenu("Test Log")]
    public void TestLog()
    {
        Debug.Log("Test log message");
    }
}