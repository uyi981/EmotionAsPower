using UnityEngine;

[CreateAssetMenu(fileName = "ResourceAttackerBehaviourSO", menuName = "Scriptable Objects/EnemyBehaviours/ResourceAttacker")]
public class ResourceAttackerBehaviourSO : EnemyBehaviourSO
{
    public override IEnemyBehaviour CreateBehaviour(Enemy enemy)
    {
        return new ResourceAttackerBehaviour(enemy);
    }
}