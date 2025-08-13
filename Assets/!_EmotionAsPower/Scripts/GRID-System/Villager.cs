using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.InferenceEngine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

[System.Serializable]
public class VillagerRuntimeData
{
    public string personalityName;
    public EmotionVector emotionVector;
    public float hunger;
    public float thirst;
    public Vector3 position;
    public string name;
    public string id;

}
public class Villager : MonoBehaviour,IInteractable
{
    public string villagerId = Guid.NewGuid().ToString();
    APathFinding pathFinding = new APathFinding();
    public bool isSelected = false;
    public bool isWorking = false;
    public bool isSleeping = false;
    public bool isStarving = false;
    public bool isPrisoner = false;
    public bool isOverControlled = false; // Flag to indicate if the villager is being controlled by the player
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
    public event Action changeEmotion;
    public event Action<Collider> chatTrigger;
    public bool isChatting;
    public string currentStateName;
    public event Action OnVillagerUpdate;
    public PlayerEmotion playerEmotion;
    public GameObject Target;
    private bool isDragging = false;
    public IState oldState;
    private float itemHeight = 0.15f; // Chiều cao 1 item
    private int currentCarryCount = 0;
    private int maxCarryCount = 6;   // Giới hạn số item có thể cầm
    void OnMouseDown()
    {
        if(isOverControlled)
        {
            return; // Prevent interaction if the villager is over-controlled
        }
        isDragging = true;
        if (Singleton<InputManagerForGrid>.Instance.CurrentState== State.Building)
        {
            return;
        }
        Debug.Log("Villager clicked: " + gameObject.name);
        // Handle villager click logic here
        if (isSelected)
        {
            Singleton<PlayerController>.Instance.RemoveVillagerOutOfList(this);
            isSelected = !isSelected;
            TransitionTo(villagerIdleState);
        }
        else
        {
            Singleton<PlayerController>.Instance.AddVillagerToList(this);
            isSelected = !isSelected;
            TransitionTo(villagerSelectedState);
        }
   
        gameObject.transform.position =Singleton<InputManagerForGrid>.Instance.GetSelectedMapPosition();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Villager"))
        {
            chatTrigger?.Invoke(other); // Notify subscribers that a villager has entered the trigger area
        }
    }
    void OnMouseUp()
    {
        isSleeping = false;
        isDragging = false;
        ReceiveEmotion(new EmotionVector(Emotion.Anger,5));
        SpriteRenderer spriteRenderer = gameObject.transform.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.color = Color.white; // Reset color if already selected
        Singleton<PlayerController>.Instance.RemoveVillagerOutOfList(this);
        isSelected = !isSelected;
        TransitionTo(villagerIdleState);
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f); // Adjust the radius as needed
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
                if(currentEmotion.Equals(Emotion.Normal))
                {
                    continue;
                }
                PrisonBuilding prisonBuilding = colliders[i].GetComponent<PrisonBuilding>();
                if (prisonBuilding != null)
                {
                    prisonBuilding.SetPrison(this);
                }
            }
        }
    }
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
    public bool CheckItemsPicked()
    {
      if(currentCarryCount>=maxCarryCount)
        {
            return true;
        }
      else        
        {
            return false;
        }
    }
    public void PickupItem(GameObject itemObj)
    {
        if (currentCarryCount >= maxCarryCount)
        {
            Debug.Log("Đã đầy túi, không thể nhặt thêm.");
            // Nếu muốn villager về, gọi TransitionTo hoặc set flag ở đây
            TransitionTo(villagerBackToHomeState);
            return;
        }

        itemObj.transform.SetParent(itemHandle.transform);

        float offsetY = currentCarryCount * itemHeight;
        itemObj.transform.localPosition = new Vector3(0, offsetY, 0);
        itemObj.transform.localRotation = Quaternion.identity;

        // Tắt physics để cố định
        var rb = itemObj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        var col = itemObj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        currentCarryCount++;

        // Nếu đạt max thì trigger hành động về
        if (currentCarryCount >= maxCarryCount)
        {
            Debug.Log("Đã nhặt đủ " + maxCarryCount + " item, quay về.");
            TransitionTo(villagerBackToHomeState);
            return;
        }
        TransitionTo(villagerIdleState); // Chuyển sang trạng thái làm việc nếu chưa đầy túi
    }

    public void DropAllItems()
    {
        for (int i = itemHandle.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = itemHandle.transform.GetChild(i);
            child.SetParent(null);
            Item item = child.GetComponent<Item>();
            if (item != null)
            {
                Singleton<ItemStorage>.Instance.AddItem(item.ItemSO, 1); // Assuming you want to add the item to a storage system
            }
            Destroy(child.gameObject);
        }
        currentCarryCount = 0;
    }
    public void BackOldState()
    {

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
        if (CurrentState == villagerStarvingState) return "Starving";
        if (CurrentState == villagerAttackEnermyState) return "Attacking";
        if (CurrentState == villagerPrisonState) return "Prison";
        // Add more states as needed                                                                                                                                                                      
        return "Unknown";
    }
    public void Update()
    {
        if (isDragging)
        {

            transform.position = Singleton<InputManagerForGrid>.Instance.GetSelectedMapPosition();
        }
        currentStateName = GetCurrentStateName();
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
        Initialize(villagerBackToHomeState); // Initialize the villager with the idle state
        TransitionTo(villagerIdleState); // Start with idle state
        playerEmotion =GetComponent<PlayerEmotion>();
        changeEmotion?.Invoke(); // Notify subscribers that the emotion has changed
        // Initialize the villager state to idle
    }
    public void HandleEmotion(Emotion currentEmotion)
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
        changeEmotion?.Invoke(); // Notify subscribers that the emotion has changed
    }
    public void ReceiveEmotion(EmotionVector emotion)
    {
        this.emotion += emotion*personality.emotionSensity; // Add the received emotion to the villager's emotion vector
        Emotion emo = this.emotion.CheckEmotion();
        currentEmotion = emo;
        HandleEmotion(emo);
        if(this.emotion.GetEmotionMaxPoint()>=80)
        {
            Debug.Log("Villager " + gameObject.name + " is over-controlled due to high emotion: " + emo);
            isOverControlled = true; // Set the villager as over-controlled if any emotion exceeds 80
        }
        else
        {
            isOverControlled = false; // Reset the over-controlled flag if no emotion exceeds 80
        }
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
        //if (CurrentState != null)
        //{
        //    CurrentState.UpdateState();
        //    currentHunger = Mathf.Clamp(currentHunger - 0.1f * personality.hungerModifier,0,100);
        //    //currentThirst -= 0.1f * personality.thirstModifier;
        //    if (currentHunger <= 20f)
        //    {
        //        if(emotion.JoyLevel >= 80)
        //        {
        //            return;
        //        }
        //        else
        //        {
        //          TransitionTo(villagerStarvingState);
        //        }
        //    }
        //    OnVillagerUpdate?.Invoke();
        //}
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
