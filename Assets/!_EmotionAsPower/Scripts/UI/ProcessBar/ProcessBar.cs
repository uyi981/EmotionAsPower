using System.Collections;
using UnityEngine;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.ProcessBar
{
    public class ProcessBar : MonoBehaviour
    {
       
        void Update()
        {
            // Luôn hướng healthBar về camera
                transform.LookAt(Camera.main.transform);
        }
    }
}