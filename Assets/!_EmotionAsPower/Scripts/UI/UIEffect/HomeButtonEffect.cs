using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeButtonEffect : MonoBehaviour, IUIEffect
{
    private TextMeshProUGUI text;
    private Button button;
    [SerializeField]
    private Color textBaseColor = Color.black;
    [SerializeField]
    private Color textHoverColor = Color.white;
    [SerializeField]
    private Sprite buttonHoverSprite;

    private void OnEnable()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        button = GetComponent<Button>();
        StopEffect();
    }
    public void PlayEffect()
    {
        text.color = textHoverColor;
        button.image.sprite = buttonHoverSprite;
        button.image.color = new Color(1, 1, 1, 1);
    }

    public void StopEffect()
    {
        text.color = textBaseColor;
        button.image.sprite = null;
        button.image.color = new Color(1, 1, 1, 0);
    }
}