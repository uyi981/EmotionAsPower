using System.Collections;
using UnityEngine;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.UIShop
{
    public class ShopUI : MonoBehaviour
    {
        [Tooltip("Công trình có được chọn hay không")]
        public bool isSelected = false;
        [Tooltip("UI công trình đang xây dựng")]
        public GameObject buildingUI;




        private void OnMouseDown()
        {
            if (!IsBuild) return; // Không cho phép chọn công trình chưa xây xong

            isSelected = !isSelected; // Đảo trạng thái chọn công trình
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}