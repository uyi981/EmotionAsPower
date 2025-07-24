using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Villager : MonoBehaviour
{
     bool isSelected = false;
    public GameObject prefab;
    void OnMouseDown()
    {
        Debug.Log("Villager clicked: " + gameObject.name);
        SpriteRenderer spriteRenderer = gameObject.transform.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.color = Color.red; // Change color to indicate selection
        // Handle villager click logic here
        if (isSelected)
        {
            spriteRenderer.color = Color.white; // Reset color if already selected
            Singleton<PlayerController>.Instance.RemoveVillagerOutOfList(this);
        }
        else
        {
            spriteRenderer.color = Color.green; // Change color to indicate selection
            Singleton<PlayerController>.Instance.AddVillagerToList(this);
            isSelected = !isSelected;
        }
    }

    //public void MoveTo(Vector3 targetPosition)
    //{
    //    // Implement movement logic here
    //    Debug.Log("Moving villager to: " + targetPosition);
    //    transform.position = targetPosition; // Simple move for demonstration
    //}
    public void Move(List<Vector2Int> path, float speed)
    {
        StartCoroutine(Moving(path,speed));
    }    
    public IEnumerator Moving(List<Vector2Int> path, float speed)
    {
        for(int i =path.Count - 1; i >= 0; i--)
        {
            Vector2Int normalPosition = NormalizeGridPosition(path[i], 100, 100); // Assuming grid size is 100x100
            Debug.Log("Moving to position: " + normalPosition);
            Vector3 targetPosition = new Vector3(normalPosition.x, -1, normalPosition.y);
            transform.position = targetPosition; // Set initial position to the first path point
            yield return new WaitForSeconds(0.5f); // Wait for the next frame
        }
        foreach (Vector2Int position in path)
        {
         
            //while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            //{
            //    transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            //}
        }
    }
    Vector2Int NormalizeGridPosition(Vector2Int pos, int gridWidth, int gridHeight)
    {
        return new Vector2Int(pos.x - gridWidth / 2, pos.y - gridHeight / 2);
    }
}
