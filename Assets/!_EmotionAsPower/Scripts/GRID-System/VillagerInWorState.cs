using UnityEngine;

public class VillagerInWorState : IState
{
    Villager villager;
    Coroutine moveCoroutine;
    public VillagerInWorState(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
       villager.Move(new Vector2Int(0, 0), villager.speed, moveCoroutine); // Assuming home is at (0, 0)
       villager.completedGoToTarget += DropItem;
    }
    public void DropItem()
    {
        if(villager.itemHandle.transform.childCount == 0)
        {
            Debug.LogWarning("No items to drop.");
            return;
        }
        Item item = villager.itemHandle.transform.GetChild(0).gameObject.GetComponent<Item>();
        if(item == null)
        {
            Debug.LogWarning("No item found in the item handle.");
            return;
        }
        Singleton<ItemStorage>.Instance.AddItem(item.ItemSO, 1); // Add item to storage
        villager.itemHandle.transform.DetachChildren(); // Detach all children from the item handle
        villager.TransitionTo(villager.villagerIdleState);
    }
    public void UpdateState()
    {
        // Logic for updating the villager selected state
    }
    public void ExitState()
    {
        villager.completedGoToTarget -= DropItem;
        // Logic for exiting the villager selected state
    }
}
