using UnityEngine;

public class EdgeCameraMovement : MonoBehaviour
{
    public float moveSpeed = 10f;      
    public float edgeSize = 30f;      
    public Vector2 moveLimitsX = new Vector2(-50, 50); 
    public Vector2 moveLimitsZ = new Vector2(-50, 50); 

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Vector3 pos = transform.position;
        Vector3 moveDir = Vector3.zero;

        Vector3 mousePos = Input.mousePosition;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Di chuyển trái
        if (mousePos.x <= edgeSize)
            moveDir += Vector3.left;

        // Di chuyển phải
        if (mousePos.x >= screenWidth - edgeSize)
            moveDir += Vector3.right;

        // Tiến
        if (mousePos.y >= screenHeight - edgeSize)
            moveDir += Vector3.forward;

        // Lùi
        if (mousePos.y <= edgeSize)
            moveDir += Vector3.back;

        // Áp dụng di chuyển
        pos += moveDir.normalized * moveSpeed * Time.deltaTime;

        // Giới hạn di chuyển trong phạm vi
        pos.x = Mathf.Clamp(pos.x, moveLimitsX.x, moveLimitsX.y);
        pos.z = Mathf.Clamp(pos.z, moveLimitsZ.x, moveLimitsZ.y);

        transform.position = pos;
    }
}
