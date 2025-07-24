using System;

[Serializable]
public class DropableItem
{
    public ItemSO item;
    public AmountChance[] amountChances;
}

[Serializable]
public class AmountChance
{
    public int amount;
    public float chance;
}