

[System.Serializable]
public struct EmotionVector
{
    public int AngerLevel;
    public int JoyLevel;
    public int SadnessLevel;
    public int FearLevel;
    public int ApatheticLevel;
    public EmotionVector(int anger, int joy, int sadness, int fear, int apathetic)
    {
        AngerLevel = anger;
        JoyLevel = joy;
        SadnessLevel = sadness;
        FearLevel = fear;
        ApatheticLevel = apathetic;
    }
}