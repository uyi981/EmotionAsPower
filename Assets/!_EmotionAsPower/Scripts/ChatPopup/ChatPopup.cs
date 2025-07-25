
using TMPro;
using UnityEngine;

public class ChatPopup : MonoBehaviour
{
    public TextMeshProUGUI text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ReturnPool()
    {
        transform.SetParent(Singleton<ChatPopupPool>.Instance.gameObject.transform);
        Singleton<ChatPopupPool>.Instance.Return(this);
    }
    public void UpdateColor(string message,int size, Color color)
    {
        text.color = color;
        text.text = message;
        text.fontSize = size;
    }
}
public enum Emotion
{
    Sad,
    Joy,
    Apethatic,
    Anger,
    Fear,
    Normal
}