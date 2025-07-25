using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

public class SelectedFrame : MonoBehaviour
{
    public GameObject[,] arraySelected = new GameObject[5, 5];
    public GameObject prefab;
    private void Start()
    {
       for (int i = 0; i < arraySelected.GetLength(0); i++)
        {
            for (int j = 0; j < arraySelected.GetLength(1); j++)
            {
                GameObject obj = Instantiate(prefab, transform);
                int x = i - (arraySelected.GetLength(0) / 2);
                int y = j - (arraySelected.GetLength(1) / 2);
                obj.transform.localPosition = new Vector3(x, 0.06f, y);
                obj.SetActive(true);
                arraySelected[i, j] = obj;
            }
        }
    }
    Vector2Int NormalizeGridPosition(Vector2Int pos, int gridWidth, int gridHeight)
    {
        return new Vector2Int(pos.x + gridWidth / 2, pos.y + gridHeight / 2);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetSize(Vector2Int size)
    {
        foreach(GameObject obj in arraySelected)
        {
            if (obj != null)
            {
              obj.SetActive(false);
            }
        }   
        for (int i = 0; i <size.x; i++)
        {
            for (int j = 0; j <size.y; j++)
            {
                if (arraySelected[i, j] != null)
                {
                    arraySelected[i, j].SetActive(true);
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            Vector2Int size = new Vector2Int(3, 3); // Example size
            SetSize(size);
        }
    }
}
