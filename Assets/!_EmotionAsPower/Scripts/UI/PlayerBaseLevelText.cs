using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class PlayerBaseLevelText : MonoBehaviour
{
    private TextMeshProUGUI label;
    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
        PlayerBase.Instance.OnLevelUpdate += UpdateText;
    }

    public void UpdateText(int level)
    {
        label.text = level.ToString();
    }
}