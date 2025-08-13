using System;
using System.Collections.Generic;
using LgTyUtils;
using UnityEngine;

[Serializable]
public class GameData
{
    public GameDateTime dateTime;
    public int playerBaseLevel;
    public float playerBaseHealth;
    public SerializableDictionary<EmotionType, int> emotionEnergy;
    public SerializableDictionary<string, int> storagedItems;
    public ItemRuntimeInstance[] items;
    public ResourceRuntimeInstance[] resources;
    public EnemyRuntimeInstance[] enemies;
    public ExplosiveRuntimeInstance[] explosives;
    public BulletRuntimeInstance[] bullets;
    public List<BuildingRuntimeData> buildings;
    public List<VillagerRuntimeData> villagers;
}