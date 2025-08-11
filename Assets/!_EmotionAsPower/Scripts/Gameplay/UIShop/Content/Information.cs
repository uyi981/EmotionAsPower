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
        public GameObject resource;


        private GameObject resourceInstance;

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

        public GameObject Resource
        {
            get => resource;
            set => resource = value;
        }

        public void OnEnable()
        {
            //if(resourceInstance != null)
            //{
            //    Destroy(resourceInstance);
            //}
            resourceInstance = Instantiate(Resource, transform);
            Debug.Log("Resource instantiated in Information component.");
        }


    }
}