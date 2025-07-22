using System;
using Unity.VisualScripting;
using UnityEngine;

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
    public bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
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
