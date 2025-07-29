using LgTyUtils;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : BaseScriptableObject
{
    public SerializableDictionary<UseCaseType, int> useCases;

    public int GetUsedValue(UseCaseType type)
    {
        if (!useCases.ContainsKey(type))
        {
            Debug.LogError($"{this.ID} cannot be used for {type.ToString()}");
            return 0;
        }
        return useCases[type];
    }

    public List<UseCaseType> GetUseCaseTypes() =>  useCases.Keys.ToList();

    public bool CanBeUsedFor(UseCaseType type) => useCases.ContainsKey(type);
}
