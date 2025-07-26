using System.Collections;
using UnityEngine;

public class VillagerChattingState : IState
{
    Villager villager;
    float time = 3;
    public VillagerChattingState(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
        time = 3;
        villager.receiveChat += OnReceiveChat;
    }
    public void OnReceiveChat(Villager sender)
    {
      villager.StartCoroutine(ReplyChat(sender));
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
    }
    public void UpdateState()
    {
        // Logic for updating the villager selected state
    }
    public void ExitState()
    {
        villager.receiveChat -= OnReceiveChat;
        // Logic for exiting the villager selected state
    }
}
public class VillagerSleepState : IState
{
    Villager villager;
    public VillagerSleepState(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
        // Logic for entering the villager sleep state
        villager.StartCoroutine(WaitForBed());
        villager.completedGoToTarget += OnGoToTarget;
    }
    public IEnumerator WaitForBed()
    {
        Debug.Log("Villager is trying to sleep.");
        yield return new WaitForSeconds(1f);
        Vector2Int getBedPosition = Singleton<VillagerManager>.Instance.GetBed();

        if(getBedPosition!=Vector2Int.zero)
        {
            Debug.Log("Villager found a bed at position: " + getBedPosition);
            villager.Move(getBedPosition,1f);
        }
        else
        {
            // Handle case where no bed is available
            Debug.LogWarning("No bed available for villager to sleep.");
        }
    }
    public void OnGoToTarget()
    {
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

        // Logic for exiting the villager sleep state
    }
}