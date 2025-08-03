using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.UIShop.Content
{
    public class IconResource : MonoBehaviour
    {
        [Header("UI References")]
        public Image image;
        public TextMeshProUGUI amountText;
        
        [Header("Data")]
        public int amount;
        public ItemSO item;

        public void Setup(ItemSO item, int amount)
        {
            this.item = item;
            this.amount = amount;
            
            // Update UI
            if (image != null && item != null)
            {
                image.sprite = item.Icon;  // Assuming ItemSO has an 'icon' field
            }
            
            if (amountText != null)
            {
                amountText.text = amount.ToString();
            }
        }
    }
}