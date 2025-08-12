using System.Collections;
using TMPro;
using UnityEngine;

public class VillagerIdleState : IState
{
    private Villager villager;
    public Coroutine moveCoroutine;
    public Coroutine checkForTargetCoroutine;
    private Vector3 targetPosition;
    private bool hasItem = false;
    public VillagerIdleState(Villager villager)
    {
      this.villager = villager;
    }
    public void OnComeTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(villager.transform.position, 2f); // Adjust the radius as needed
        for (int i = colliders.Length - 1; i >= 0; i--)
        {
            Collider collider = colliders[i];
            if (collider.CompareTag("Item"))
            {
                villager.PickupItem(collider.gameObject);
                return;

            }
            else if (collider.CompareTag("Resource"))
            {
                villager.Target = collider.gameObject; // Set the target to the resource
                villager.TransitionTo(villager.villagerAttackEnermyState);
                return;

            }
            else if (collider.CompareTag("Enermy") || collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                villager.Target = collider.gameObject; // Set the target to the resource
                villager.TransitionTo(villager.villagerAttackEnermyState);
                return;
            }
            else if (collider.CompareTag("Building"))
            {
                villager.Target = collider.gameObject; // Set the target to the resource
                villager.TransitionTo(villager.villagerAttackEnermyState);
                return;
            }
        }
        if(moveCoroutine==null)
        moveCoroutine = villager.StartCoroutine(MoveToRandomPointRoutine()); // Restart moving to random points if no item found
    }
    public void ResetMovingRandom()
    {
        if (moveCoroutine != null)
            villager.StopCoroutine(moveCoroutine); // Stop any existing movement coroutine\
        moveCoroutine = null;
    }
    public IEnumerator CheckForTarget()
    {
        Debug.Log("Starting CheckForTarget for Villager: " + villager.gameObject.name);
        hasItem = false;
        while (!hasItem)
        {
            Debug.Log("Villager: " + villager.gameObject.name + " is checking for items or resources.");
            //Debug.Log("Checking for items or resources...");
            yield return new WaitForSeconds(0.2f); // Check every 0.5 seconds
            Collider[] colliders = Physics.OverlapSphere(villager.transform.position, 5f); // Adjust the radius as needed
            for(int i = colliders.Length - 1; i >= 0; i--)
            {
                Collider collider = colliders[i];
                if(villager.isOverControlled&&villager.currentEmotion.Equals(Emotion.Anger))
                {
                    if (collider.CompareTag("Building"))
                    {
                        Vector3Int itemPosition = Singleton<GridSystem>.Instance.grid.WorldToCell(collider.transform.position);
                        Vector2Int villagerPosition = new Vector2Int(itemPosition.x, itemPosition.z);
                        ResetMovingRandom();
                        villager.Move(villagerPosition, villager.personality.moveSpeedModifier);
                        hasItem = true;
                        break;
                    }
                }
                else
                {
                    if (collider.CompareTag("Item"))
                    {
                        Vector3Int itemPosition = Singleton<GridSystem>.Instance.grid.WorldToCell(collider.transform.position);
                        Vector2Int villagerPosition = new Vector2Int(itemPosition.x, itemPosition.z);
                        ResetMovingRandom();
                        villager.Move(villagerPosition, villager.personality.moveSpeedModifier);
                        hasItem = true;
                        break;
                    }
                    else if (collider.CompareTag("Resource"))
                    {
                        Vector3Int villagerPosition = Singleton<GridSystem>.Instance.grid.WorldToCell(collider.transform.position);
                        Vector2Int targetPosition = new Vector2Int(villagerPosition.x, villagerPosition.z);
                        ResetMovingRandom();
                        villager.Move(targetPosition, villager.personality.moveSpeedModifier);
                        hasItem = true;
                        break;
                    }
                    else if (collider.CompareTag("Enermy")||collider.gameObject.layer==LayerMask.NameToLayer("Enemy"))
                    {
                        Debug.Log("Found Enermy: " + collider.gameObject.name);
                        Vector3Int villagerPosition = Singleton<GridSystem>.Instance.grid.WorldToCell(collider.transform.position);
                        Vector2Int targetPosition = new Vector2Int(villagerPosition.x, villagerPosition.z);
                        ResetMovingRandom();
                        villager.Move(targetPosition, villager.personality.moveSpeedModifier);
                        hasItem = true;
                        break;
                    }
                }
           
            }
        }
        hasItem = false; // Reset the flag after checking
    }
    public void OnCollisionEnter(Collision collision)
    {
        //if(!collision.gameObject.CompareTag("Item")||!collision.gameObject.CompareTag("Villager"))
        //{
        //    return;
        //}
        if(collision.gameObject.CompareTag("Villager"))
        {
            Debug.Log("Collided with Villager: " + collision.gameObject.name);
            CheckIsChatable(collision.gameObject.GetComponent<Villager>());
            return;
        }
        else if(collision.gameObject.CompareTag("Item"))
        {
          villager.PickupItem(collision.gameObject);
        }
    }
    public void CheckIsChatable(Villager otherVillager)
    {
        Debug.Log("Checking if villagers can chat: " + villager.name +villager.isChatting + " and " + otherVillager.name + otherVillager.isChatting);
        if (otherVillager.isChatting)
        {
            return;
        }
       if(villager.isChatting)
        {
            return;
        }
        SendMessageToOtherVillager(otherVillager);
    }
    public void SendMessageToOtherVillager(Villager otherVillager)
    {
       int number =  Random.Range(0, 100);
        if(number<=villager.personality.rateSendChat*100)
        {
            int number2 = Random.Range(0, 100);
            if(number2<=otherVillager.personality.rateAcceptChat*100)
            {
                Debug.Log("Villager " + villager.name + " is chatting with " + otherVillager.name);
                villager.isChatting = true;
                otherVillager.isChatting = true;
                villager.TransitionTo(villager.villagerChattingState);
                otherVillager.TransitionTo(otherVillager.villagerChattingState);
                otherVillager.ReceiveChat(villager);
            }
        }
    }
    IEnumerator MoveToRandomPointRoutine()
    {
        while (true)
        {
            // Chọn vị trí ngẫu nhiên trong không gian 3D
            targetPosition = new Vector3(
                Random.Range(-5, 5),
                Random.Range(0, 0), // có thể điều chỉnh trục Y tùy game
                Random.Range(-5, 5)
            );

            // Di chuyển dần đến vị trí đích
            while (Vector3.Distance(villager.transform.position, targetPosition) > 0.1f)
            {
                villager.transform.position = Vector3.MoveTowards(
                    villager.transform.position,
                    targetPosition,
                    villager.personality.moveSpeedModifier * Time.deltaTime
                );
                yield return null;
            }
            // Dừng 2s trước khi chọn điểm tiếp theo
            yield return new WaitForSeconds(0.5f);
        }
    }
    public void EnterState()
    {
        Debug.Log("Enter Idle State for Villager: " + villager.gameObject.name);

        moveCoroutine = villager.StartCoroutine(MoveToRandomPointRoutine());
        checkForTargetCoroutine = villager.StartCoroutine(CheckForTarget()); // Start checking for items
        villager.completedGoToTarget+= OnComeTarget; // Subscribe to the event when the villager reaches the target
        villager.isWorking = false; // Set working state to false
        villager.collisionTrigger += OnCollisionEnter; // Subscribe to the collision event
        if (villager.CheckItemsPicked())
        {
            villager.TransitionTo(villager.villagerBackToHomeState); // Transition to working state if items are picked
            return;
        }
    }
    public void UpdateState()
    {
    
    }
    public void ExitState()
    {
        villager.completedGoToTarget -= OnComeTarget; // Subscribe to the event when the villager reaches the target
        villager.collisionTrigger -= OnCollisionEnter; // Unsubscribe from the collision event
        if (moveCoroutine != null)
            villager.StopCoroutine(moveCoroutine);
        if (checkForTargetCoroutine != null)
            villager.StopCoroutine(checkForTargetCoroutine);
        // Implement logic for exiting the idle state
    }
}
