using System.Collections;
using UnityEngine;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.Building.Interface
{
    public interface IProduction
    {
        /// <summary>
        /// Bắt đầu quá trình sản xuất.
        /// </summary>
        void StartProduction();
        /// <summary>
        /// Dừng quá trình sản xuất.
        /// </summary>
        void StopProduction();
        /// <summary>
        /// Kiểm tra xem có thể sản xuất hay không.
        /// </summary>
        /// <returns>Trả về true nếu có thể sản xuất, ngược lại false.</returns>
        bool CanProduce();
    }
}