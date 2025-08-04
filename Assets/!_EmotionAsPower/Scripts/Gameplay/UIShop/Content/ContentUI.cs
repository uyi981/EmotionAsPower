using System.Collections;
using LgTyUtils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.UIShop.Content
{
    public class ContentUI : MonoBehaviour
    {
        [Header("IconPrefab Settings")]
        public GameObject iconUI;


        [Header("Content Settings")]
        public TextMeshProUGUI textMeshPro;
        public GameObject iconBuildings;




        private GameObject iconUIInstance;
        public TextMeshProUGUI TextMeshPro { get; set; }

        public GameObject InstantiateIcon()
        {
            iconUIInstance = Instantiate(iconUI, iconBuildings.transform);
            //iconUIInstance.GetComponent<IconUI>().InstantiateResource();
            return iconUIInstance;
        }

        public void SetImage(Sprite sprite)
        {

            var iconImage = iconUIInstance.GetComponent<IconUI>();
            if (iconImage != null)
            {
                iconImage.iconImage.sprite = sprite;
            }
        }

        public void SetResource(SerializableDictionary<ItemSO, int> resources)
        {
            iconUIInstance.GetComponent<IconUI>().SetResource(resources);
        }


        public void SetInformation(GameObject info, Sprite img, string text, string des) { 
            var iconUI = iconUIInstance.GetComponent<IconUI>();
            iconUI.information = info;
            var information = iconUI.information.GetComponent<Information>();
            
            information.Image.sprite = img;
            information.Name.text = text;
            information.Description.text = des;
        }
    }
}