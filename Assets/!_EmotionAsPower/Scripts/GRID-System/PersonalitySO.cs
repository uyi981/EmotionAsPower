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
    public float hungerModifier; // how much hunger will be affected by emotion
    public float thirstModifier; // how much thirst will be affected by emotion
    public float tiredModifier; // how much tiredness will be affected by emotion
    public float moveSpeedModifier; // how much movement speed will be affected by emotion
}
