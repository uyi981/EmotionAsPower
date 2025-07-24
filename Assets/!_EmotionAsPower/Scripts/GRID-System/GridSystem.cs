using UnityEngine;

public class GridSystem : Singleton<GridSystem>
{
    public Grid grid;
    public float[,] gridMap = new float[100, 100];
    private void Start()
    {
        for(int x = 0; x < gridMap.GetLength(0); x++)
        {
            for (int y = 0; y < gridMap.GetLength(1); y++)
            {
                gridMap[x, y] = 0f; // Initialize all cells to walkable
            }
        }
    }
}
