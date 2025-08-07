

using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct EmotionVector
{
    public float AngerLevel;
    public float JoyLevel;
    public float SadnessLevel;
    public float FearLevel;
    public float ApatheticLevel;
    public EmotionVector(float anger, float joy, float sadness, float fear, float apathetic)
    {
        AngerLevel = anger;
        JoyLevel = joy;
        SadnessLevel = sadness;
        FearLevel = fear;
        ApatheticLevel = apathetic;
    }
    public Emotion CheckEmotion()
    {
        float max = Mathf.Max(AngerLevel, JoyLevel, SadnessLevel, FearLevel, ApatheticLevel);
        if (max >= 20)
        {
            switch (max)
            {
                case float m when m == AngerLevel:
                    return Emotion.Anger;
                case float m when m == JoyLevel:
                    return  Emotion.Joy;
                case float m when m == SadnessLevel:
                    return Emotion.Sad;
                case float m when m == FearLevel:
                    return Emotion.Fear;
                case float m when m == ApatheticLevel:
                    return Emotion.Apethatic;
                default:
                    return Emotion.Normal;
            }
        }
        return Emotion.Normal;
    }
    public float GetEmotionMaxPoint()
    {
        float max = Mathf.Max(AngerLevel, JoyLevel, SadnessLevel, FearLevel, ApatheticLevel);
        return max;
    }
    public static EmotionVector operator +(EmotionVector a, EmotionVector b)
    {
        return new EmotionVector(
            a.AngerLevel + b.AngerLevel,
            a.JoyLevel + b.JoyLevel,
            a.SadnessLevel + b.SadnessLevel,
            a.FearLevel + b.FearLevel,
            a.ApatheticLevel + b.ApatheticLevel
        );
    }
    public static EmotionVector operator *(EmotionVector a, int multiplier)
    {
        return new EmotionVector(
            a.AngerLevel * multiplier,
            a.JoyLevel * multiplier,
            a.SadnessLevel * multiplier,
            a.FearLevel * multiplier,
            a.ApatheticLevel * multiplier
        );
    }
    public static EmotionVector operator *(EmotionVector a, EmotionVector b)
    {
        return new EmotionVector(
           a.AngerLevel * b.AngerLevel,
            a.JoyLevel * b.JoyLevel,
            a.SadnessLevel * b.SadnessLevel,
            a.FearLevel * b.FearLevel,
            a.ApatheticLevel * b.ApatheticLevel
        );
    }

}