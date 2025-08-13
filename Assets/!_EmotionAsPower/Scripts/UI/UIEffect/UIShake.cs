using UnityEngine;

public class UIShake : MonoBehaviour
{
    public Vector2 shakeVector = new Vector2(5f, 5f); 
    public float shakeSpeed = 10f;

    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if(rect == null)
        {
            rect=GetComponent<RectTransform>();
        }
        float t = Time.time * shakeSpeed;
        rect.anchoredPosition += new Vector2(
            Mathf.Sin(t) * shakeVector.x,
            Mathf.Cos(t) * shakeVector.y
        );
    }
}
