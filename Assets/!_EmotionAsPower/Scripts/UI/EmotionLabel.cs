using LgTyUtils;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class EmotionLabel : MonoBehaviour
{
    [SerializeField]
    private Emotion emotion;
    [SerializeField]
    private TextMeshProUGUI text;
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();    
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ItemStorage.Instance.OnStoragedItemsChange += SetEmotionAmount;
    }

    public void SetEmotionAmount(SerializableDictionary<string, int> items)
    {
        string emotionID = EmotionHelper.GetEmotionID(emotion);
        if (items.ContainsKey(emotionID)) {
            text.text = items[emotionID].ToString();
        }
        else
        {
            text.text = "0";
        }
        
    }

    private void OnDestroy()
    {
        //EmotionEnergyManager.Instance.OnEmotionEnergyChange -= SetEmotionAmount;
    }
}
