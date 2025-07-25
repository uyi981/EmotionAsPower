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