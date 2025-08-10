

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
    public EmotionVector(Emotion emotion, float value)
    {
        AngerLevel = emotion == Emotion.Anger ? value : 0f;
        JoyLevel = emotion == Emotion.Joy ? value : 0f;
        SadnessLevel = emotion == Emotion.Sad ? value : 0f;
        FearLevel = emotion == Emotion.Fear ? value : 0f;
        ApatheticLevel = emotion == Emotion.Apethatic ? value : 0f;
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
                    return Emotion.Joy;
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
    public float minusEmotion(Emotion emotion, float value)
    {
        switch (emotion)
        {
            case Emotion.Anger:
                AngerLevel = Mathf.Max(0, AngerLevel - value);
                return AngerLevel;
            case Emotion.Joy:
                JoyLevel = Mathf.Max(0, JoyLevel - value);
                return JoyLevel;
            case Emotion.Sad:
                SadnessLevel = Mathf.Max(0, SadnessLevel - value);
                return SadnessLevel;
            case Emotion.Fear:
                FearLevel = Mathf.Max(0, FearLevel - value);
                return FearLevel;
            case Emotion.Apethatic:
                ApatheticLevel = Mathf.Max(0, ApatheticLevel - value);
                return ApatheticLevel;
            default:
                return 0f;
        }
      
    }
    public static EmotionVector operator +(EmotionVector a, EmotionVector b)
    {
        return new EmotionVector(
            Mathf.Clamp(a.AngerLevel + b.AngerLevel, 0, 100),
            Mathf.Clamp(a.JoyLevel + b.JoyLevel, 0, 100),
            Mathf.Clamp(a.SadnessLevel + b.SadnessLevel, 0, 100),
            Mathf.Clamp(a.FearLevel + b.FearLevel, 0, 100),
            Mathf.Clamp(a.ApatheticLevel + b.ApatheticLevel, 0, 100)
        );
    }
    public static EmotionVector operator *(EmotionVector a, int multiplier)
    {
        return new EmotionVector(
            Mathf.Clamp(a.AngerLevel * multiplier, 0, 100),
            Mathf.Clamp(a.JoyLevel * multiplier, 0, 100),
            Mathf.Clamp(a.SadnessLevel * multiplier, 0, 100),
            Mathf.Clamp(a.FearLevel * multiplier, 0, 100),
            Mathf.Clamp(a.ApatheticLevel * multiplier, 0, 100)
        );
    }
    public static EmotionVector operator *(EmotionVector a, EmotionVector b)
    {
        return new EmotionVector(
            Mathf.Clamp(a.AngerLevel * b.AngerLevel, 0, 100),
            Mathf.Clamp(a.JoyLevel * b.JoyLevel, 0, 100),
            Mathf.Clamp(a.SadnessLevel * b.SadnessLevel, 0, 100),
            Mathf.Clamp(a.FearLevel * b.FearLevel, 0, 100),
            Mathf.Clamp(a.ApatheticLevel * b.ApatheticLevel, 0, 100)
        );
    }

}