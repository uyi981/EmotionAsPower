using UnityEngine;
using UnityEngine.UI;

namespace Assets.__EmotionAsPower.Scripts.UI.ProcessBar
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        public void SetHealth(float normalized)
        {
            if (fillImage != null)
                fillImage.fillAmount = normalized;
        }
    }
}
