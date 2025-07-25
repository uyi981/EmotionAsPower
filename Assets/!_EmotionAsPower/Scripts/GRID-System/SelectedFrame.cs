using System.Collections.Generic;
using UnityEngine;

public class SelectedFrame : MonoBehaviour
{
    public GameObject[,] arraySelected;
    public GameObject prefab;
    private List<GameObject> pool = new List<GameObject>();


    public void SetSize(Vector2Int size)
    {
        int total = size.x * size.y;

        while (pool.Count < total)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Add(obj);
        }

        foreach (var obj in pool)
        {
            obj.SetActive(false);
        }

        arraySelected = new GameObject[size.x, size.y];
        int idx = 0;
        float offsetX = (size.x - 1) * 0.5f;
        float offsetY = (size.y - 1) * 0.5f;
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                GameObject obj = pool[idx++];
                float x = i - offsetX;
                float y = j - offsetY;
                obj.transform.localPosition = new Vector3(x, 0.06f, y);
                obj.SetActive(true);
                arraySelected[i, j] = obj;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Vector2Int size = new Vector2Int(3, 3); // Example size
            SetSize(size);
        }
    }
}
