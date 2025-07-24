using UnityEngine;
using LgTyUtils;

public abstract class EnemyBehaviourSO : ScriptableObject
{
    public abstract IEnemyBehaviour CreateBehaviour(Enemy enemy);
}