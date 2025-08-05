using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.__EmotionAsPower.Scripts.UI.ProcessBar
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI health;

        public void SetHealth(int health)
        {
            this.health.text = health.ToString();
            Debug.Log("Health set to: " + health);
        }
    }
}
