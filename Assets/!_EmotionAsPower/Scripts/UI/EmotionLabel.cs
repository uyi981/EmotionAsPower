using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class EmotionLabel : MonoBehaviour
{
    [SerializeField]
    private EmotionType targetEmotion;
    [SerializeField]
    private TextMeshProUGUI text;
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();    
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EmotionEnergyManager.Instance.OnEmotionEnergyChange += SetEmotionAmount;
    }

    public void SetEmotionAmount(EmotionType emotion, int amount)
    {
        if(emotion == targetEmotion)
        {
            text.text = amount.ToString();
        }    
    }

    private void OnDestroy()
    {
        //EmotionEnergyManager.Instance.OnEmotionEnergyChange -= SetEmotionAmount;
    }
}
