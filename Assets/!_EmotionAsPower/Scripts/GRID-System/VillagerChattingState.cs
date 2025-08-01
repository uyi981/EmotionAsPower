using System.Collections;
using TMPro;
using UnityEngine;

public class VillagerChattingState : IState
{
    Villager villager;
    Coroutine chatCoroutine;
    float time = 3;
    public VillagerChattingState(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
        time = 3;
        villager.receiveChat += OnReceiveChat;
        Debug.Log("Villager is entering chatting state.");
    }
    public void OnReceiveChat(Villager sender)
    {
        Debug.Log("Villager received chat from: " + sender.name);
        chatCoroutine = villager.StartCoroutine(ReplyChat(sender));
    }
    public IEnumerator ReplyChat(Villager sender)
    {
        yield return new WaitForSeconds(1f);
        time -= 1;
        if (time == 0)
        {
            villager.TransitionTo(villager.villagerIdleState);
            sender.TransitionTo(sender.villagerIdleState);
            villager.isChatting = false;
            sender.isChatting = false;
            sender.ReceiveEmotion(villager.personality.emotionSendAffterChat);
            villager.ReceiveEmotion(sender.personality.emotionSendAffterChat);
        }
        ChatPopup cp = Singleton<ChatPopupPool>.Instance.Get(Emotion.Normal, "meow");
        cp.transform.position = villager.transform.position;
        cp.transform.SetParent(villager.itemHandle.transform);
        sender.ReceiveChat(villager);
        chatCoroutine = null;
    }
    public void UpdateState()
    {
        // Logic for updating the villager selected state
    }
    public void ExitState()
    {
        villager.isChatting = false;
        villager.receiveChat -= OnReceiveChat;
        if(chatCoroutine != null)
            villager.StopCoroutine(chatCoroutine); // Stop the chat coroutine if it is running
        // Logic for exiting the villager selected state
    }
}
public class VillagerSleepState : IState
{
    Villager villager;
    Coroutine sleepCoroutine;
    public VillagerSleepState(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
        // Logic for entering the villager sleep state
        Debug.Log("Villager is entering sleep state.");
        villager.StartCoroutine(WaitForBed());
        villager.completedGoToTarget += OnGoToTarget;
      
        villager.isWorking = true;
        villager.isSleeping = true;
    }
    public IEnumerator WaitForBed()
    {
        Debug.Log("Villager is trying to sleep.");
        yield return new WaitForSeconds(0.1f);
        Vector2Int getBedPosition = Singleton<VillagerManager>.Instance.GetBed();

        if(getBedPosition!=Vector2Int.zero)
        {
            Debug.Log("Villager found a bed at position: " + getBedPosition);
            villager.Move(getBedPosition,1f);
        }
        else
        {
            // Handle case where no bed is available
        }
    }
    public IEnumerator Sleep()
    {
        while (true)
        {
            ChatPopup cp = Singleton<ChatPopupPool>.Instance.Get(Emotion.Normal, "zZzZ");
            cp.transform.position = villager.transform.position;
            cp.transform.SetParent(villager.itemHandle.transform);
            yield return new WaitForSeconds(3f);
        }
    }
    public void OnGoToTarget()
    {
        sleepCoroutine = villager.StartCoroutine(Sleep());
        // Logic for when the villager reaches the target position

    }
    public void UpdateState()
    {
        // Logic for updating the villager sleep state
    }
    public void ExitState()
    {
        villager.completedGoToTarget -= OnGoToTarget;
        if(villager.moveCoroutine != null)
        {
            villager.StopCoroutine(villager.moveCoroutine); // Stop any existing movement coroutine
        }
        if(sleepCoroutine != null)
        {
            villager.StopCoroutine(sleepCoroutine); // Stop the sleep coroutine
            sleepCoroutine = null;
        }
        // Logic for exiting the villager sleep state
    }
}
public class VillagerStarvingState : IState
{
    Villager villager;
    Coroutine hungryCoroutine;
    Coroutine moveCoroutine;
    Vector3 targetPosition;
    public VillagerStarvingState(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
        // Logic for entering the villager starving state
        Debug.Log("Villager is entering starving state.");
        villager.isStarving = true;
        hungryCoroutine = villager.StartCoroutine(Hungry());
        moveCoroutine = villager.StartCoroutine(MoveToRandomPointRoutine());
        villager.collisionTrigger +=Eat; // Subscribe to collision events for eating
        // Additional logic for handling starvation
    }
    public void Eat(Collision collision)
    {
        if (collision.gameObject.CompareTag("Food"))
        {
            Debug.Log("Food: " + collision.gameObject.name);
            ItemSO itemSO = collision.gameObject.GetComponent<Item>().ItemSO;
            if (itemSO.useCases.Pairs[0].key.Equals(UseCaseType.Eat))
            {
              villager.currentHunger = Mathf.Clamp(villager.currentHunger+itemSO.useCases.Pairs[0].value, 0, 100); // Ensure hunger does not exceed 100
                GameObject.Destroy(collision.gameObject); // Destroy the food item
            }
            else
            {
                Debug.LogWarning("Item cannot be eaten.");
            }
        }
        else
        {
            return;
        }
    }
    IEnumerator MoveToRandomPointRoutine()
    {
        while (true)
        {
            // Chọn vị trí ngẫu nhiên trong không gian 3D
            targetPosition = new Vector3(
                Random.Range(-20, 20),
                Random.Range(0, 0), // có thể điều chỉnh trục Y tùy game
                Random.Range(-20, 20)
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
    public IEnumerator Hungry()
    {
        while(true)
        {
            ChatPopup cp = Singleton<ChatPopupPool>.Instance.Get(Emotion.Normal, "Hungry Hungry!!");
            cp.transform.position = villager.transform.position;
            cp.transform.SetParent(villager.itemHandle.transform);
            yield return new WaitForSeconds(1f);
        }
    }
    public void UpdateState()
    {
      if(villager.currentHunger>=50)
        {
            villager.TransitionTo(villager.villagerIdleState); // Transition to idle state if hunger is above 50
        }
        // Logic for updating the villager starving state
    }
    public void ExitState()
    {
        villager.isStarving = false;
        if (hungryCoroutine != null)
        {
            villager.StopCoroutine(hungryCoroutine); // Stop the hungry coroutine
            hungryCoroutine = null;
        }
        if (moveCoroutine != null)
        {
            villager.StopCoroutine(moveCoroutine); // Stop the move coroutine
            moveCoroutine = null;
        }
        villager.collisionTrigger -= Eat;
        // Logic for exiting the villager starving state
    }
}