using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class VillagerRuntimeData
{
    public string personalityName;
    public EmotionVector emotionVector;
    public float hunger;
    public float thirst;
    Vector3 position;
    public string name;

}
public class Villager : MonoBehaviour,IInteractable
{
    APathFinding pathFinding = new APathFinding();
    public bool isSelected = false;
    public bool isWorking = false;
    public bool isSleeping = false;
    public bool isStarving = false;
    public bool isPrisoner = false;
    public Animator animator;
    public Emotion currentEmotion = Emotion.Normal;
    public JobForWorker currentJob;
    public EmotionVector emotion = new EmotionVector();
    public VillagerWorkingState villagerWorkingState;
    public VillagerIdleState villagerIdleState;
    public villagerSelectedState villagerSelectedState;
    public VillagerBackToHomeState villagerBackToHomeState;
    public VillagerChattingState villagerChattingState;
    public VillagerSleepState villagerSleepState;
    public VillagerStarvingState villagerStarvingState;
    public VillagerAttackEnermyState villagerAttackEnermyState;
    public villagerPrisonState villagerPrisonState;
    public PersonalitySO personality;
    IState CurrentState;
    public Coroutine moveCoroutine;
    public GameObject itemHandle;
    public event Action completedGoToTarget;
    public event Action<Collision> collisionTrigger;
    public event Action<Villager> receiveChat;
    public bool isChatting;
    public string currentStateName;
    public float currentHunger = 100f;
    public float currentThirst = 100f;
    public event Action OnVillagerUpdate;
    public PlayerEmotion playerEmotion;
    public GameObject Target;
    private bool isDragging = false;

    void OnMouseDown()
    {
        isDragging = true;
        if (Singleton<InputManagerForGrid>.Instance.CurrentState== State.Building)
        {
            return;
        }
        Debug.Log("Villager clicked: " + gameObject.name);
        SpriteRenderer spriteRenderer = gameObject.transform.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.color = Color.red; // Change color to indicate selection
        // Handle villager click logic here
        if (isSelected)
        {
            spriteRenderer.color = Color.white; // Reset color if already selected
            Singleton<PlayerController>.Instance.RemoveVillagerOutOfList(this);
            isSelected = !isSelected;
            TransitionTo(villagerIdleState);
        }
        else
        {
            spriteRenderer.color = Color.green; // Change color to indicate selection
            Singleton<PlayerController>.Instance.AddVillagerToList(this);
            isSelected = !isSelected;
            TransitionTo(villagerSelectedState);
        }
   
        gameObject.transform.position =Singleton<InputManagerForGrid>.Instance.GetSelectedMapPosition();
    }
    void OnMouseUp()
    {
        isDragging = false;
        SpriteRenderer spriteRenderer = gameObject.transform.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.color = Color.white; // Reset color if already selected
        Singleton<PlayerController>.Instance.RemoveVillagerOutOfList(this);
        isSelected = !isSelected;
        TransitionTo(villagerIdleState);
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f); // Adjust the radius as needed
        for (int i = colliders.Length - 1; i >= 0; i--)
        {
            if (colliders[i].gameObject.tag.Equals("Factory"))
            {
                ProductionBuilding productionBuilding = colliders[i].GetComponent<ProductionBuilding>();
                if (productionBuilding != null)
                {
                    productionBuilding.CheckIsHaveEmptyJob(this);
                }
            }
            else if (colliders[i].gameObject.tag.Equals("PrisonBuilding"))
            {
                PrisonBuilding prisonBuilding = colliders[i].GetComponent<PrisonBuilding>();
                if (prisonBuilding != null)
                {
                    prisonBuilding.SetPrison(this);
                }
            }
        }
    }    
    //public float GetEmotionPoint(Emotion emotion)
    //{
    //    switch (emotion)
    //    {
    //        case Emotion.Joy:
    //            return this.emotion.JoyLevel;
    //        case Emotion.Sad:
    //            return this.emotion.SadnessLevel;
    //        case Emotion.Anger:
    //            return this.emotion.AngerLevel;
    //        case Emotion.Fear:
    //            return this.emotion.FearLevel;
    //        case Emotion.Apethatic:
    //            return this.emotion.ApatheticLevel;
    //        case Emotion.Normal:
    //        default:
    //            return  0;
    //    }
    //}
    public void OnDayStageChange(DayTimeController.TimeStage timeStage)
    {
        if (timeStage == DayTimeController.TimeStage.Morning)
        {

        }
        else if (timeStage == DayTimeController.TimeStage.Evening)
        {
            currentJob.Position = Vector2Int.zero;
            currentJob.JobType = JobType.None;
        }
    }

    public void Hunger()
    {

    }
    public string GetCurrentStateName()
    {
        if (CurrentState == villagerIdleState) return "Idle";
        if (CurrentState == villagerWorkingState) return "Working";
        if (CurrentState == villagerSelectedState) return "Selected";
        if (CurrentState == villagerBackToHomeState) return "BackToHome";
        if (CurrentState == villagerChattingState) return "Chatting";
        if (CurrentState == villagerSleepState) return "Sleeping";
        return "Unknown";
    }
    public void Update()
    {
        if (isDragging)
        {
         
            transform.position = Singleton<InputManagerForGrid>.Instance.GetSelectedMapPosition();
        }
        currentStateName = GetCurrentStateName();
        if(Input.GetKeyDown(KeyCode.Space))
        {

                Debug.Log("Villager " + gameObject.name + " is attacking!");
                animator.Play("Attack");
        }
    }
    private void Start()
    {
        animator = transform.GetComponentInChildren<Animator>();
        villagerWorkingState = new VillagerWorkingState(this);
        villagerIdleState = new VillagerIdleState(this);
        villagerSelectedState = new villagerSelectedState(this);
        villagerBackToHomeState = new VillagerBackToHomeState(this);
        villagerChattingState = new VillagerChattingState(this);
        villagerSleepState = new VillagerSleepState(this);
        villagerStarvingState = new VillagerStarvingState(this);
        villagerAttackEnermyState = new VillagerAttackEnermyState(this);
        villagerPrisonState = new villagerPrisonState(this);
        InvokeRepeating("UpdateState", 0f, 0.1f);
        currentHunger = 25f;
        Initialize(villagerBackToHomeState); // Initialize the villager with the idle state
        TransitionTo(villagerIdleState); // Start with idle state
        playerEmotion =GetComponent<PlayerEmotion>();
        // Initialize the villager state to idle
    }
    void HandleEmotion(Emotion currentEmotion)
    {
        SpriteRenderer spriteRenderer = gameObject.transform.GetComponentInChildren<SpriteRenderer>();
        switch (currentEmotion)
        {
            case Emotion.Joy:
                // Ví dụ: NPC cười, vẫy tay, chạy nhanh
                playerEmotion.SetEmotion(Emotion.Joy, Color.yellow);
                break;

            case Emotion.Sad:
                // Ví dụ: NPC chậm chạp, cúi đầu
                playerEmotion.SetEmotion(Emotion.Sad, Color.blue);
                break;

            case Emotion.Anger:
                // Ví dụ: NPC đỏ mặt, nói gắt, đấm tường
                playerEmotion.SetEmotion(Emotion.Anger, Color.orangeRed);
                break;

            case Emotion.Fear:
                // Ví dụ: NPC rung, bỏ chạy, né xa player
                playerEmotion.SetEmotion(Emotion.Fear, Color.lawnGreen);

                break;

            case Emotion.Apethatic:
                // Ví dụ: NPC không phản ứng gì, đứng yên
                playerEmotion.SetEmotion(Emotion.Apethatic, Color.gray);
                break;

            case Emotion.Normal:
            default:
                // NPC hoạt động bình thường
                playerEmotion.SetEmotion(Emotion.Normal, Color.white);
                break;
        }
    }
    public void ReceiveEmotion(EmotionVector emotion)
    {
        this.emotion += emotion*personality.emotionSensity; // Add the received emotion to the villager's emotion vector
        Emotion emo = this.emotion.CheckEmotion();
        currentEmotion = emo;
        HandleEmotion(emo);
    }
 
    public void Initialize(IState startingState)
    {
        CurrentState = startingState;
        CurrentState.ExitState();
    }
    public void TransitionTo(IState nextState)
    {
        //if(CurrentState.Equals(nextState))
        //{
        //    return; // No transition needed if the next state is the same as the current state
        //}
        if(isPrisoner)
        {
            return;
        }
        if (isSelected && !nextState.Equals(villagerSelectedState))
        {
            return;
        }
        if(isSleeping)
        {
            return;
        }
        if(CurrentState!=null)
        {
            CurrentState.ExitState();
            CurrentState = nextState;
            nextState.EnterState();
        }
    }
    public void UpdateState()
    {
        if(isSleeping||isStarving)
        {
            return;
        }
        if (CurrentState != null)
        {
            CurrentState.UpdateState();
            currentHunger = Mathf.Clamp(currentHunger - 0.1f * personality.hungerModifier,0,100);
            //currentThirst -= 0.1f * personality.thirstModifier;
            if (currentHunger <= 20f)
            {
                TransitionTo(villagerStarvingState);
            }
            OnVillagerUpdate?.Invoke();
        }
    }
    public void Move(Vector2Int targetPosition, float speed)
    {
        Vector3Int villagerPosition = Singleton<GridSystem>.Instance.grid.WorldToCell(transform.position);
        //Debug.Log(villagerPosition);
        Vector2Int startPosition = new Vector2Int(villagerPosition.x, villagerPosition.z);
        //Debug.Log("Before Start Position: " + startPosition);
        //Debug.Log("Before Target Position: " + targetPosition);
        List<Vector2Int> path = pathFinding.GetPathResult(VoHauMethod.NormalizeGridPosition(startPosition, 100, 100), VoHauMethod.NormalizeGridPosition(targetPosition, 100, 100), Singleton<GridSystem>.Instance.gridMap, 1);
        if (path != null && path.Count > 0)
        {

            if (moveCoroutine!= null)
            {
                StopCoroutine(moveCoroutine); // Stop any existing movement coroutine
            }
            moveCoroutine = StartCoroutine(Moving(path, speed));
        }
        else
        {
            Debug.LogWarning("No valid path found for villager to move from " + startPosition + " to " + targetPosition);
            // Handle case where no path is found
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        collisionTrigger?.Invoke(collision); // Notify subscribers that a job has been taken
    }
    public void ReceiveChat(Villager sender)
    {
        receiveChat?.Invoke(sender);
    }
    public IEnumerator Moving(List<Vector2Int> path, float speed)
    {
        for (int i = path.Count - 1; i >= 0; i--)
        {

            Vector2Int normalPosition = VoHauMethod.InverseNormalizeGridPosition(path[i], 100, 100); // Assuming grid size is 100x100
            Vector3 targetPosition = new Vector3(normalPosition.x, 0, normalPosition.y);
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition,personality.moveSpeedModifier * Time.deltaTime);
                yield return null;
            }          
        }
        completedGoToTarget?.Invoke(); // Notify subscribers that the villager has come home
        moveCoroutine = null; // Reset coroutine reference after movement is complete
    }

    public void OnInteract()
    {
       
    }

    public InteractableType GetInteractableType()
    {
       return InteractableType.Enemy;
    }
}
