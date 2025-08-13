using UnityEngine;

[CreateAssetMenu(fileName = "NewPersonality", menuName = "EmotionAsPower/Emotion")]
public class EmotionSO : ScriptableObject
{
    public Emotion id;
    public int hungerModifier; // how much hunger will be affected by emotion
    public float moveSpeedModifier; // how much movement speed will be affected by emotion
    public float worKSpeedModifier; // how much work speed will be affected by emotion
    public float maxCarryModifier;
}
