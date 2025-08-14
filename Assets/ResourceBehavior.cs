using UnityEngine;

public class ResourceBehavior : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3Int vector3Int = new Vector3Int((int)gameObject.transform.position.x, 0, (int)gameObject.transform.position.z);
        OccupyCells(vector3Int, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OccupyCells(Vector3Int basePos, int check)
    {
        for (int dx = 0; dx < 1; dx++)
        {
            Debug.Log($"currentSize = {1}");
            for (int dz = 0; dz <1; dz++)
            {
                int gx = basePos.x + dx;
                int gz = basePos.z + dz;
                Debug.Log($"Occupying cell at: {gx}, {gz} with value {check}");
               Singleton<GridSystem>.Instance.gridMap[gx + 50, gz + 50] = check;
            }
        }
    }
}
