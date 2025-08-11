using System.Text.RegularExpressions;
using UnityEngine;

public enum ItemCategory
{
    Default = 0,

    Stone = 2, 
    ConstructionMaterial = 3, 
    Food = 4,
    Special = 5,

    Emotion = 1,
}

public static class ItemCategoryExtensions
{
    public static string ToDisplayString(this ItemCategory category)
    {
        string enumName = category.ToString();
        return Regex.Replace(enumName, @"([a-z])([A-Z])", "$1 $2");
    }
}
