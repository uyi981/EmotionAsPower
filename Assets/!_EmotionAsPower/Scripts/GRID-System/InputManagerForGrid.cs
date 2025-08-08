using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManagerForGrid : MonoBehaviour
{
    [SerializeField]
    private Camera sceneCamera;
    [SerializeField] Vector3 lasPosition;
    [SerializeField] LayerMask placementLayerMask;
    public event Action OnClicked;
    public event Action OnRightClicked;

    public event Action OnExit;
    public State CurrentState { get; set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit,100, placementLayerMask))
        {
            lasPosition = hit.point;
            return lasPosition;
        }
        else
        {
            return lasPosition; // Return last valid position if no hit
        }
    }
    [SerializeField] private GraphicRaycaster raycaster;
    [SerializeField] private EventSystem eventSystem;
    public bool IsPointerOverUI()
    {
        if (raycaster == null)
            raycaster = FindFirstObjectByType<GraphicRaycaster>();

        if (eventSystem == null)
            eventSystem = EventSystem.current;

        // Tạo danh sách kết quả raycast
        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        // Nếu có trúng UI
        if (results.Count > 0)
        {
            Debug.Log("Pointer over UI:");
            foreach (var result in results)
            {
                Debug.Log(result.gameObject.name); // Tên object UI
            }

            return true;
        }

        return false;
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 selectedPosition = GetSelectedMapPosition();
            Debug.Log("Selected Position: " + selectedPosition);
            OnClicked?.Invoke();
        }
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 selectedPosition = GetSelectedMapPosition();
            Debug.Log("Selected Position: " + selectedPosition);
            OnRightClicked?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnExit?.Invoke();
        }

    }
}
public enum State
{
    Moving,
    Building,
}
