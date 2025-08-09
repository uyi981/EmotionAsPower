using System.Collections;
using UnityEngine;

namespace Assets.__EmotionAsPower.Scripts.Gameplay.Building.BuidingChild
{
    public class BreedingBuilding : BuildingBase
    {
        public VillagerManager villagerManager;
        public Villager villagerPrefab;

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
        public void OpenBreedingUI()
        {
           Singleton<UIManager>.Instance.OpenBreedingUI(this);
        }
        public  void Breeding(string name)
        {
            PersonalitySO personality = Singleton<PersonalitySystem>.Instance.Breeding();
            Villager villagerData = Instantiate(villagerPrefab, transform.position - Vector3.forward * 2f, Quaternion.identity, villagerManager.transform);
            villagerData.personality = personality;
            villagerData.name = name;
            Singleton<VillagerManager>.Instance.AssginNewVillager(villagerData);
        }
    }
}