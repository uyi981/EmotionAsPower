using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Villager : MonoBehaviour
{
    APathFinding pathFinding = new APathFinding();
    bool isSelected = false;
    public bool isWorking = false;
    public JobForWorker currentJob;
    int AngerLevel = 0;
    int JoyLevel = 0;
    int SadnessLevel = 0;
    int FearLevel = 0;
    int ApatheticLevel = 0;
    public VillagerWorkingState villagerWorkingState;
    public VillagerIdleState villagerIdleState;
    public villagerSelectedState villagerSelectedState;
    public VillagerBackToHomeState villagerBackToHomeState;
    IState CurrentState;
    Coroutine moveCoroutine;
    public event Action OnTakeItem;
    public GameObject itemHandle;
    public event Action OnComeHome;
    void TakeJob(JobForWorker currentJob)
    {
        currentJob = currentJob;
        OnTakeItem?.Invoke(); // Notify subscribers that a job has been taken
    }
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
    private void Start()
    {
        villagerWorkingState = new VillagerWorkingState(this);
        villagerIdleState = new VillagerIdleState(this);
        villagerSelectedState = new villagerSelectedState(this);
        villagerBackToHomeState = new VillagerBackToHomeState(this);
        InvokeRepeating("UpdateState", 0f, 0.1f);
        Initialize(villagerIdleState);
        TransitionTo(villagerIdleState); // Start with idle state
        // Initialize the villager state to idle
    }
    public void Initialize(IState startingState)
    {
        CurrentState = startingState;
        startingState.ExitState();
    }
    public void TransitionTo(IState nextState)
    {
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
        Vector2Int startPosition = new Vector2Int(villagerPosition.x, villagerPosition.z);
        Debug.Log("Before Start Position: " + startPosition);
        Debug.Log("Before Target Position: " + targetPosition);
        Debug.Log("start"+NormalizeGridPosition(startPosition, 500, 500));
        Debug.Log("target"+NormalizeGridPosition(targetPosition, 500, 500));
        List<Vector2Int> path = pathFinding.GetPathResult(NormalizeGridPosition(startPosition, 100, 100), NormalizeGridPosition(targetPosition, 100, 100), Singleton<GridSystem>.Instance.gridMap, 1);
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

        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        collision.transform.SetParent(itemHandle.transform); // Set the collided object as a child of the villager
        collision.transform.localPosition = Vector3.zero; // Reset position to the villager's position
        collision.collider.enabled = false; // Disable the collider to prevent further collisions
        collision.rigidbody.useGravity = false; // Disable gravity for the collided object
        OnTakeItem?.Invoke(); // Notify subscribers that a job has been taken
    }
    public IEnumerator Moving(List<Vector2Int> path, float speed)
    {
        for (int i = path.Count - 1; i >= 0; i--)
        {

            Vector2Int normalPosition = InverseNormalizeGridPosition(path[i], 100, 100); // Assuming grid size is 100x100
            Vector3 targetPosition = new Vector3(normalPosition.x, 0, normalPosition.y);
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, 5f * Time.deltaTime);
                yield return null;
            }          
        }
        OnComeHome?.Invoke(); // Notify subscribers that the villager has come home
        moveCoroutine = null; // Reset coroutine reference after movement is complete
    }
    Vector2Int InverseNormalizeGridPosition(Vector2Int pos, int gridWidth, int gridHeight)
    {
        return new Vector2Int(pos.x - gridWidth / 2, pos.y - gridHeight / 2);
    }
    Vector2Int NormalizeGridPosition(Vector2Int pos, int gridWidth, int gridHeight)
    {
        return new Vector2Int(pos.x + gridWidth / 2, pos.y + gridHeight / 2);
    }
}
public class villagerSelectedState : IState
{
    Villager villager;
    public villagerSelectedState(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
        // Logic for entering the villager selected state
    }
    public void UpdateState()
    {
        // Logic for updating the villager selected state
    }
    public void ExitState()
    {
        // Logic for exiting the villager selected state
    }
}
public class VillagerBackToHomeState : IState
{
    Villager villager;
    public VillagerBackToHomeState(Villager villager)
    {
        this.villager = villager;
    }
    public void EnterState()
    {
       villager.Move(new Vector2Int(0, 0), 1f); // Assuming home is at (0, 0)
       villager.OnComeHome += DropItem;
    }
    public void DropItem()
    {
        villager.itemHandle.transform.DetachChildren(); // Detach all children from the item handle
        villager.TransitionTo(villager.villagerIdleState);
    }
    public void UpdateState()
    {
        // Logic for updating the villager selected state
    }
    public void ExitState()
    {
        villager.OnComeHome -= DropItem;
        // Logic for exiting the villager selected state
    }
}


