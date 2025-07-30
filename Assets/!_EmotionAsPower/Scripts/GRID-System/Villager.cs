using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Villager : MonoBehaviour,IInteractable
{
    APathFinding pathFinding = new APathFinding();
    public bool isSelected = false;
    public bool isWorking = false;
    public Emotion currentEmotion = Emotion.Normal;
    public JobForWorker currentJob;
    EmotionVector emotion = new EmotionVector();
    public VillagerWorkingState villagerWorkingState;
    public VillagerIdleState villagerIdleState;
    public villagerSelectedState villagerSelectedState;
    public VillagerBackToHomeState villagerBackToHomeState;
    public VillagerChattingState villagerChattingState;
    public VillagerSleepState villagerSleepState;
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

    void OnMouseDown()
    {
        if(Singleton<InputManagerForGrid>.Instance.CurrentState== State.Building)
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
        currentStateName = GetCurrentStateName();
    }
    private void Start()
    {
        villagerWorkingState = new VillagerWorkingState(this);
        villagerIdleState = new VillagerIdleState(this);
        villagerSelectedState = new villagerSelectedState(this);
        villagerBackToHomeState = new VillagerBackToHomeState(this);
        villagerChattingState = new VillagerChattingState(this);
        villagerSleepState = new VillagerSleepState(this);
        InvokeRepeating("UpdateState", 0f, 0.1f);
        Initialize(villagerIdleState);
        TransitionTo(villagerIdleState); // Start with idle state
        // Initialize the villager state to idle
    }
    void HandleEmotion(Emotion currentEmotion)
    {
        SpriteRenderer spriteRenderer = gameObject.transform.GetComponentInChildren<SpriteRenderer>();
        switch (currentEmotion)
        {
            case Emotion.Joy:
                // Ví dụ: NPC cười, vẫy tay, chạy nhanh
                spriteRenderer.color = Color.yellow; // Change color to indicate selection
                break;

            case Emotion.Sad:
                // Ví dụ: NPC chậm chạp, cúi đầu
                spriteRenderer.color = Color.blue; // Change color to indicate selection
                break;

            case Emotion.Anger:
                // Ví dụ: NPC đỏ mặt, nói gắt, đấm tường
                spriteRenderer.color = Color.orangeRed; // Change color to indicate selection
                break;

            case Emotion.Fear:
                // Ví dụ: NPC rung, bỏ chạy, né xa player
                spriteRenderer.color = Color.lawnGreen; // Change color to indicate selection

                break;

            case Emotion.Apethatic:
                // Ví dụ: NPC không phản ứng gì, đứng yên
                spriteRenderer.color = Color.gray; // Change color to indicate selection
                break;

            case Emotion.Normal:
            default:
                // NPC hoạt động bình thường
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
        startingState.ExitState();
    }
    public void TransitionTo(IState nextState)
    {
        if (isSelected && !nextState.Equals(villagerSelectedState))
        {
            return;
        }
        CurrentState.ExitState();
        CurrentState = nextState;
        nextState.EnterState();
    }
    public void UpdateState()
    {
        if (CurrentState != null)
        {
            CurrentState.UpdateState();
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
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, 5f * Time.deltaTime);
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
