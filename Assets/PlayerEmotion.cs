using System.Collections.Generic;
using UnityEngine;

public class PlayerEmotion : MonoBehaviour
{
    public GameObject joyFace;
    public GameObject sadFace;
    public GameObject angryFace;
    public GameObject neutralFace;
    public GameObject boringFace;
    public GameObject scareFace;
    public SpriteRenderer body;
    public SpriteRenderer head;
    public Dictionary<Emotion, GameObject> emotionFaces = new Dictionary<Emotion, GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        emotionFaces.Add(Emotion.Joy, joyFace);
        emotionFaces.Add(Emotion.Sad, sadFace);
        emotionFaces.Add(Emotion.Anger, angryFace);
        emotionFaces.Add(Emotion.Normal, neutralFace);
        emotionFaces.Add(Emotion.Apethatic, boringFace);
        emotionFaces.Add(Emotion.Fear, scareFace);
        SetEmotion(Emotion.Normal,Color.white); // Set default emotion to Normal
    }
    public void SetEmotion(Emotion emotion,Color color)
    {
        foreach (var face in emotionFaces.Values)
        {
            face.SetActive(false);
        }
        if (emotionFaces.ContainsKey(emotion))
        {
            emotionFaces[emotion].SetActive(true);
        }
        body.color = color;
    }
}
