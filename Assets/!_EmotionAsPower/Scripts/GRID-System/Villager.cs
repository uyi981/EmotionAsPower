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
    public bool isChatting = false; // Flag to indicate if the villager is currently chatting
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
    public VillagerLeavingState villagerLeavingState;
    public float maxDistanceFromCenter = 30f; // Adjust as needed
    public villagerPrisonState villagerPrisonState;
    public PersonalitySO personality;
    public EmotionSO emotionSO;
    public IState CurrentState;
    public Coroutine moveCoroutine;
    public GameObject itemHandle;
    public event Action completedGoToTarget;
    public event Action<Collision> collisionTrigger;
    public event Action<Villager> receiveChat;
    public event Action changeEmotion;
    public event Action<Collider> chatTrigger;
    public string currentStateName;
    public event Action OnVillagerUpdate;
    public PlayerEmotion playerEmotion;
    public GameObject Target;
    private bool isDragging = false;
    public IState oldState;
    private float itemHeight = 0.15f; // Chiều cao 1 item
    private int currentCarryCount = 0;

    public float speed { get { return personality.moveSpeedModifier * emotionSO.moveSpeedModifier; } }
    public float workSpeed { get { return personality.workSpeedModifier * emotionSO.worKSpeedModifier; } }
    public float hungerModifier { get { return Mathf.RoundToInt(personality.hungerModifier*emotionSO.hungerModifier); } }
    public int maxCarryCount { get { return Mathf.RoundToInt(personality.maxCarryModifier * emotionSO.maxCarryModifier)*2; } }
    public void SetPersonality(PersonalitySO personality)
    {
        this.personality = personality;

    }
    void OnMouseDown()
    {
        isDragging = true;
        Singleton<DetailInfoController>.Instance.OpenVillageUI(this);
        gameObject.transform.position =Singleton<InputManagerForGrid>.Instance.GetSelectedMapPosition();
        if(CurrentState.Equals(villagerWorkingState))
        {
            TransitionTo(villagerIdleState); // Transition to selected state when clicked
        }    
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
            else if (colliders[i].gameObject.tag.Equals("PlayerBase"))
            {
                DropAllItems();
            }
            else if (colliders[i].gameObject.CompareTag("Item"))
            {
                PickupItem(colliders[i].gameObject);
            }
            else if (colliders[i].gameObject.tag.Equals("PrisonBuilding"))
            {
                if (currentEmotion.Equals(Emotion.Normal))
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
            // Nếu muốn villager về, gọi TransitionTo hoặc set flag ở đây
            //   TransitionTo(villagerBackToHomeState);
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

        if (currentCarryCount >= maxCarryCount)
        {
            TransitionTo(villagerIdleState);
            // TransitionTo(villagerBackToHomeState);
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
        if (CurrentState == villagerLeavingState) return "Leaving";
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
        villagerLeavingState = new VillagerLeavingState(this);
        InvokeRepeating("UpdateState", 0f,5f);
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
                emotionSO = Singleton<PersonalitySystem>.Instance.GetEmotionModifier(Emotion.Joy);
                break;

            case Emotion.Sad:
                // Ví dụ: NPC chậm chạp, cúi đầu
                playerEmotion.SetEmotion(Emotion.Sad, Color.blue);
                emotionSO = Singleton<PersonalitySystem>.Instance.GetEmotionModifier(Emotion.Sad);
                break;

            case Emotion.Anger:
                // Ví dụ: NPC đỏ mặt, nói gắt, đấm tường
                playerEmotion.SetEmotion(Emotion.Anger, Color.orangeRed);
                emotionSO = Singleton<PersonalitySystem>.Instance.GetEmotionModifier(Emotion.Anger);
                break;

            case Emotion.Fear:
                // Ví dụ: NPC rung, bỏ chạy, né xa player
                playerEmotion.SetEmotion(Emotion.Fear, Color.lawnGreen);
                emotionSO = Singleton<PersonalitySystem>.Instance.GetEmotionModifier(Emotion.Fear);
                break;

            case Emotion.Apethatic:
                // Ví dụ: NPC không phản ứng gì, đứng yên
                playerEmotion.SetEmotion(Emotion.Apethatic, Color.gray);
                emotionSO = Singleton<PersonalitySystem>.Instance.GetEmotionModifier(Emotion.Apethatic);
                break;

            case Emotion.Normal:
            default:
                // NPC hoạt động bình thường
                playerEmotion.SetEmotion(Emotion.Normal, Color.white);
                emotionSO = Singleton<PersonalitySystem>.Instance.GetEmotionModifier(Emotion.Normal);
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
    }
 
    public void Initialize(IState startingState)
    {
        CurrentState = startingState;
        CurrentState.ExitState();
    }
    public void TransitionTo(IState nextState)
    {
        //if (CurrentState.Equals(nextState))
        //{
        //    return; // No transition needed if the next state is the same as the current state
        //}
        if (isPrisoner)
        {
            return;
        }
        if(isChatting)
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
        //float distance = Vector3.Distance(transform.position, Vector3.zero);
        //if (distance > maxDistanceFromCenter)
        //{
        //    // Increase fear points by 1 (or any value you want)
        //    emotion += new EmotionVector(Emotion.Fear, 1f);
        //}
        //if (emotion.FearLevel > 80f || emotion.AngerLevel > 80f)
        //{
        //    TransitionTo(villagerLeavingState);
        //    return;
        //}
        SendEmotionToGod();
    }
    public void SendEmotionToGod()
    {
        if(currentEmotion.Equals(Emotion.Normal))
        {
            return; // Không gửi emotion nếu là Emotion.Normal
        }
        Singleton<ItemStorage>.Instance.AddItem(EmotionHelper.GetEmotionID(currentEmotion),1);
        emotion.minusEmotion(currentEmotion, 1); // Giảm emotion vector
        ReceiveEmotion(new EmotionVector(currentEmotion,0)); // Cập nhật emotion vector
    }
    public void ResetCoroutine(Coroutine coroutine)
    {
        if(coroutine!=null)
        {
            StopCoroutine(coroutine); // Stop the coroutine if it is running
            coroutine = null; // Reset the coroutine reference
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
            Debug.Log("is stucll");

            if (moveCoroutine!= null)
            {
                StopCoroutine(moveCoroutine); // Stop any existing movement coroutine
            }
            moveCoroutine = StartCoroutine(Moving(path, speed));
        }
        else
        {
            Debug.LogWarning("No valid path found for villager to move from " + startPosition + " to " + targetPosition);
            TransitionTo(villagerIdleState); // Transition to idle state if no path is found
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
        float timeOut = 1;
        for (int i = path.Count - 1; i >= 0; i--)
        {

            Vector2Int normalPosition = VoHauMethod.InverseNormalizeGridPosition(path[i], 100, 100); // Assuming grid size is 100x100
            Vector3 targetPosition = new Vector3(normalPosition.x, 0, normalPosition.y);
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition,personality.moveSpeedModifier * Time.deltaTime);
                //timeOut-=Time.deltaTime;
                //if(timeOut <= 0)
                //{
                //    Debug.LogWarning("Villager " + gameObject.name + " movement timed out at position: " + targetPosition);
                //    yield break; // Exit the coroutine if movement takes too long
                //}
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
