using System.Collections.Generic;
using UnityEngine;

public class ChatPopupPool : Singleton<ChatPopupPool>
{
    public Stack<ChatPopup> pool = new Stack<ChatPopup>();
    public ChatPopup sample;
    private int size = 32;
    public ChatPopup Get(Emotion type, string message)
    {
        size = 6;
        if (pool.Count > 0)
        {
            ChatPopup go = pool.Pop();
            go.UpdateColor(message, size, Color.white);
            go.gameObject.SetActive(true);
            return go;
        }
        else
        {
            ChatPopup go = Instantiate(sample);
            go.UpdateColor(message, size, Color.white);
            return go;
        }
    }
    public void Return(ChatPopup popup)
    {
        popup.gameObject.SetActive(false);
        pool.Push(popup);
    }
    public void Clear()
    {
        while (pool.Count > 0)
        {
            ChatPopup popup = pool.Pop();
            Destroy(popup.gameObject);
        }
    }
}
    