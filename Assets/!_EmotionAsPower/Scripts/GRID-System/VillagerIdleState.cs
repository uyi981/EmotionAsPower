using System.Collections;
using TMPro;
using UnityEngine;

public class VillagerIdleState : IState
{
    private Villager villager;
    public Coroutine moveCoroutine;
    private Vector3 targetPosition;
    public VillagerIdleState(Villager villager)
    {
      this.villager = villager;
    }
    public void OnCollisionEnter(Collision collision)
    {
        //if(!collision.gameObject.CompareTag("Item")||!collision.gameObject.CompareTag("Villager"))
        //{
        //    return;
        //}
        if(collision.gameObject.CompareTag("Villager"))
        {
            CheckIsChatable(collision.gameObject.GetComponent<Villager>());
            return;
        }
        else if(collision.gameObject.CompareTag("Item"))
        {
            collision.transform.SetParent(villager.itemHandle.transform); // Set the collided object as a child of the villager
            collision.transform.localPosition = Vector3.zero; // Reset position to the villager's position
            collision.collider.enabled = false; // Disable the collider to prevent further collisions
            collision.rigidbody.useGravity = false; // Disable gravity for the collided object
            BackToHome();
        }     
    }
    public void CheckIsChatable(Villager otherVillager)
    {
       if(otherVillager.isChatting)
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
    public void BackToHome()
    {
        villager.TransitionTo(villager.villagerBackToHomeState); // Transition to working state when item is taken
    }
    public void EnterState()
    {
        moveCoroutine = villager.StartCoroutine(MoveToRandomPointRoutine());
        villager.isWorking = false; // Set working state to false
        villager.collisionTrigger += OnCollisionEnter; // Subscribe to the collision event
        if(villager.itemHandle.transform.childCount > 0)
        {
            BackToHome();
        }
    }
    public void UpdateState()
    {
     
    }
    public void ExitState()
    {
        villager.collisionTrigger -= OnCollisionEnter; // Unsubscribe from the collision event
        if (moveCoroutine != null)
            villager.StopCoroutine(moveCoroutine);
        moveCoroutine = null;
        // Implement logic for exiting the idle state
    }
}
