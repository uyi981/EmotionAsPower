using UnityEngine;

public static class VoHauMethod
{
    public static Vector2Int InverseNormalizeGridPosition(Vector2Int pos, int gridWidth, int gridHeight)
    {
        return new Vector2Int(pos.x - gridWidth / 2, pos.y - gridHeight / 2);
    }
    public static Vector2Int NormalizeGridPosition(Vector2Int pos, int gridWidth, int gridHeight)
    {
        return new Vector2Int(pos.x + gridWidth / 2, pos.y + gridHeight / 2);
    }
}
