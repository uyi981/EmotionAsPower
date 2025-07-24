using UnityEngine;

[CreateAssetMenu(fileName = "ItemDestroyerBehaviourSO", menuName = "Scriptable Objects/EnemyBehaviours/ItemDestroyer")]
public class ItemDestroyerBehaviourSO : EnemyBehaviourSO
{
    public override IEnemyBehaviour CreateBehaviour(Enemy enemy)
    {
        return new ItemDestroyerBehaviour(enemy);
    }
}