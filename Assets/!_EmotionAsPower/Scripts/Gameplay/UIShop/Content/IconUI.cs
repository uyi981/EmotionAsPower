using System.Collections;
using System.Collections.Generic;
using Assets.__EmotionAsPower.Scripts.Gameplay.UIShop.Content;
using LgTyUtils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.UIShop
{
    public class IconUI : MonoBehaviour
    {
        [Header("Prefab ")]
        public GameObject iconResourcePrefab;
        public GameObject resource;
        public GameObject information;

        [Header("Icon Settings")]
        public Image iconImage;


        


        private GameObject iconResourcePrefabInstance;
        private Button button;

        private void Start()
        {
            information = GameObject.FindWithTag("Info");

            information.SetActive(false); // Ensure information panel is initially hidden
            // Add button component if not exists
            button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }
            
            // Set up click listener
            button.onClick.AddListener(OnIconClick);
        }
        
        private void OnIconClick()
        {
            Debug.Log("Icon clicked!");
           information.SetActive(true);
            if (information == null)
            {
                Debug.LogWarning("Information panel is not assigned in the inspector!");
                return;
            }
            
            // Create the information panel if it doesn't exist
            if (information.transform.parent != transform)
            {
                information = Instantiate(information, transform);
                information.transform.localPosition = Vector3.zero; // Reset position
            }
            
            // Toggle the information panel
            information.SetActive(!information.activeSelf);
        }


        public GameObject InstantiateResource()
        {
            
            //Instantiate(iconResourcePrefabInstance, iconResourcePrefabInstance.transform);
            return iconResourcePrefabInstance;
        }


        public void SetResource(SerializableDictionary<ItemSO, int> resourceData)
        {
            if (resourceData != null)
            {
                    

                    // Set up new resources
                    foreach (var item in resourceData)
                    {
                        Debug.Log($"Setting resource: {item.Key} with amount: {item.Value}");
                        var resourceIcon = Instantiate(iconResourcePrefab, resource.transform);
                        var itemIcon = resourceIcon.GetComponent<IconResource>();
                        if (itemIcon != null && item.Key != null)
                        {
                            itemIcon.Setup(item.Key, item.Value);
                        }
                    }
            }
            else
            {
                Debug.LogWarning("Cannot set resource: iconResourcePrefabInstance is null or resource data is invalid");
            }
        }
    }
}