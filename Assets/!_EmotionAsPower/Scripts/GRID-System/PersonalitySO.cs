using UnityEngine;

[CreateAssetMenu(fileName = "NewPersonality", menuName = "EmotionAsPower/Personality")]
public class PersonalitySO :ScriptableObject
{
    public string name;
    [TextArea(20,10)]
    public string description;
    public float rateSendChat;
    public float rateAcceptChat;
    public EmotionVector emotionSendAffterChat; // will send emotion after chat
    public EmotionVector emotionSensity; //will analyze emotionSendAffterChat from other NPC
    public int hungerModifier; // how much hunger will be affected by emotion
    public float moveSpeedModifier; // how much movement speed will be affected by emotion
    public float worKSpeedModifier; // how much work speed will be affected by emotion
    public float maxCarryModifier;
}
