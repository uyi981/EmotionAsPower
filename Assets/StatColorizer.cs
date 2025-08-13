using UnityEngine;
using UnityEngine.UI; // Nếu dùng UI Text cũ
using TMPro; // Nếu dùng TextMeshPro

public class StatColorizer : MonoBehaviour
{
    [SerializeField] private TMP_Text statText; // Nếu dùng TextMeshPro
    //[SerializeField] private Text statText; // Nếu dùng UI Text cũ

    private void Start()
    {
        ApplyColor();
    }

    public void ApplyColor()
    {
        if (statText == null) return;

        string text = statText.text.Trim();

        if (text.StartsWith("+"))
        {
            statText.color = Color.green; // Màu xanh nếu là cộng
        }
        else if (text.StartsWith("-"))
        {
            statText.color = Color.red; // Màu đỏ nếu là trừ
        }
        else
        {
            statText.color = Color.white; // Mặc định
        }
    }
}
