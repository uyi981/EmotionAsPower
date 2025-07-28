using System.Collections.Generic;

[System.Serializable]
public class EnemyWaveConfig
{
    public string waveName;
    public List<EnemySpawnGroup> spawnGroups = new List<EnemySpawnGroup>();
    public bool useFormation = false;
    public float formationRadius = 10f;
    public float spawnRadius = 15f;
}
