using System.Collections;
using UnityEngine;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.Building.Interface
{
    public interface IDefense
    {
        /// <summary>
        /// Bắt đầu quá trình phòng thủ.
        /// </summary>
        void StartDefense();
        /// <summary>
        /// Dừng quá trình phòng thủ.
        /// </summary>
        void StopDefense();
        /// <summary>
        /// Kiểm tra xem có thể phòng thủ hay không.
        /// </summary>
        /// <returns>Trả về true nếu có thể phòng thủ, ngược lại false.</returns>
        bool CanDefend();
    }
}