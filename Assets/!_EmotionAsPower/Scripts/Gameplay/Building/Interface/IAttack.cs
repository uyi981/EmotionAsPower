using System.Collections;
using UnityEngine;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.Building.Interface
{
    public interface IAttack
    {
        int AttackDamage { get; set; }
        int AttackRange { get; set; }
        float AttackCooldown { get; set; }
        void Attack(Transform target);
        
    }
}