using UnityEngine;

public static class EmotionHelper
{
    public static ItemSO GetEmotion(Emotion emotion)
    {
        string itemID = GetEmotionID(emotion);

        return ContentManager.Instance.ItemSOs[itemID];
    }

    public static string GetEmotionID(Emotion emotion)
    {
        return $"Emotion_{emotion.ToString()}";
    }
}