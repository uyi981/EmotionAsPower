using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.UIShop.Content
{
    public class Information : MonoBehaviour
    {
        [Header("UI References Object")]
        public Image image;
        public TextMeshProUGUI name;
        public TextMeshProUGUI description;
        

        public Image Image
        {
            get => image;
            set => image = value;
        }

        public TextMeshProUGUI Name
        {
            get => name;
            set => name = value;
        }
        public TextMeshProUGUI Description
        {
            get => description;
            set => description = value;
        }


       
    }
}