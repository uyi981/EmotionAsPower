using System.Collections;
using UnityEngine;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.Building.Interface
{
    public interface IAttack
    {
        
        void Attack(GameObject target);
        
        bool CanAttack(GameObject target);
    }
}