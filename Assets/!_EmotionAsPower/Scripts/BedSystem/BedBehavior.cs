using UnityEngine;

public class BedBehavior : MonoBehaviour
{
    public Vector2Int position;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      Vector3Int vector3 =Singleton<GridSystem>.Instance.grid.WorldToCell(transform.position);
      position = new Vector2Int(vector3.x, vector3.z);
      Singleton<DayTimeController>.Instance.OnTimeStageChanged += OnDayStageChange; 
    }
    public void OnDayStageChange(DayTimeController.TimeStage timeStage)
    {
        if (timeStage == DayTimeController.TimeStage.Evening)
        {      
            Singleton<VillagerManager>.Instance.AssignFreeBed(position);
        }
    }
}
