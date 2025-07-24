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
                    5 * Time.deltaTime
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
        villager.OnTakeItem += BackToHome; // Subscribe to the event

    }
    public void UpdateState()
    {
     
    }
    public void ExitState()
    {
        villager.OnTakeItem -= BackToHome; // Unsubscribe from the event
        if (moveCoroutine != null)
            villager.StopCoroutine(moveCoroutine);
        moveCoroutine = null;
        // Implement logic for exiting the idle state
    }
}
