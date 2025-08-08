using System.Collections;
using UnityEngine;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.Building.BuidingChild
{
    public class BreedingBuilding : BuildingBase
    {
        public VillagerManager villagerManager;
        public GameObject villagerPrefab;

        private void Start()
        {
            base.Start();
            // Initialize the villagerManager if not set
            if (villagerManager == null)
            {
                villagerManager = FindFirstObjectByType<VillagerManager>();
                if (villagerManager == null)
                {
                    Debug.LogError("VillagerManager not found in the scene!");
                }
            }
        }
        public void Breed()
        {
            // Instantiate 1 cube 
            Instantiate(villagerPrefab, transform.position - Vector3.forward * 2f, Quaternion.identity, villagerManager.transform);
        }
    }
}