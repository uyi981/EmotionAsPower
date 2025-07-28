using UnityEngine;

[CreateAssetMenu(fileName = "Destroy Item Action", menuName = "Scriptable Objects/AI/Actions/Destroy Item")]
public class DestroyItemAction : AIAction
{
    [Header("Item Destruction Settings")]
    [SerializeField] private float destructionRange = 1f;
    [SerializeField] private Vector3 fallbackPosition = Vector3.zero;

    public override bool CanPerform(AIController controller)
    {
        Item target = TargetFinder.FindNearestTarget<Item>(controller.transform.position);
        return target != null;
    }

    public override void StartAction(AIController controller)
    {
        Item target = TargetFinder.FindNearestTarget<Item>(controller.transform.position);
        if (target != null)
        {
            controller.ActionData.target = target.transform;

            // If close enough, destroy immediately, if not then move
            float distance = Vector3.Distance(controller.transform.position, target.transform.position);
            if (distance <= destructionRange)
            {
                DestroyItem(target);
            }
            else
            {
                controller.ActionData.targetPosition = target.transform.position;
            }
        }
        else
        {
            // No items found, move to fallback position
            controller.ActionData.targetPosition = fallbackPosition;
        }
    }

    public override ActionResult UpdateAction(AIController controller)
    {
        if (controller.ActionData.target != null)
        {
            Item item = controller.ActionData.target.GetComponent<Item>();
            if (item == null)
            {
                return ActionResult.Success; // Item was destroyed
            }

            float distance = Vector3.Distance(controller.transform.position, controller.ActionData.target.position);
            if (distance <= destructionRange)
            {
                DestroyItem(item);
                return ActionResult.Success;
            }
            else
            {
                // Move to item
                if (!controller.unitMover.IsMoving())
                {
                    controller.unitMover.MoveToWorldPosition(controller.ActionData.target.position);
                }
                return ActionResult.Running;
            }
        }
        else{ 
            return ActionResult.Failed; 
        }
    }
    private void DestroyItem(Item item)
    {
        if (item != null)
        {
            item.Clear();
            if (debugMode)
                Debug.Log($"Destroyed item: {item.name}");
        }
    }
}