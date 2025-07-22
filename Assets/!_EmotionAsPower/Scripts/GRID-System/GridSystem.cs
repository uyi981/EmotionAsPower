using UnityEngine;

public class GridSystem : Singleton<GridSystem>
{
    public Grid grid;
    public float[,] gridMap = new float[100, 100];  
}
