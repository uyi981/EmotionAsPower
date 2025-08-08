using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.__EmotionAsPower.Scripts.UI.ProcessBar
{
    public class ProcessBar : MonoBehaviour
    {

        [SerializeField] private Image fillImage;

        public void SetProcess(float normalized)
        {
            if (fillImage != null)
                fillImage.fillAmount = normalized;
        }
        
    }
}