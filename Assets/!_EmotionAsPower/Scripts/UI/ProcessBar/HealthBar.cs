using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.__EmotionAsPower.Scripts.UI.ProcessBar
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI health;
        [SerializeField] private TextMeshProUGUI maxHealth;
        [SerializeField] private Image fillImage;

        public void SetProcess(float normalized)
        {
            if (fillImage != null)
                fillImage.fillAmount = normalized;
        }


        public TextMeshProUGUI Health
        {
            get => health;
            set => health = value;
        }

        public TextMeshProUGUI MaxHealth
        {
            get => maxHealth;
            set => maxHealth = value;
        }


        public void Instantiate(int health, int maxhealth)
        {
            this.health.text = health.ToString();
            this.maxHealth.text = maxhealth.ToString();
        }

    }
}
