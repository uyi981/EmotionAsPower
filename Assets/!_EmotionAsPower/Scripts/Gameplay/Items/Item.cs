using TMPro;
using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    [Header("Item properties")]
    [SerializeField]
    private ItemSO itemSO;
    public ItemSO ItemSO => itemSO;
    [SerializeField]
    private int amount;
    public int Amount => amount;

    [Header("Item material")]
    [SerializeField]
    private string textureProperty;
    
    public void Initialize(ItemSO itemSO, int amount)
    {
        this.gameObject.name = itemSO.DisplayName + "_" + amount;

        this.amount = amount;
        this.itemSO = itemSO;

        SetMaterial();

        ItemGroupHandler itemGroupHandler  = GetComponentInChildren<ItemGroupHandler>();
        itemGroupHandler.SetItem(this);

        TextMeshProUGUI amountText = GetComponentInChildren<TextMeshProUGUI>();
        amountText.text = amount.ToString();

    }

    public void OnInteract()
    {
        Debug.Log($"Item: {itemSO.DisplayName}");
    }

    public InteractableType GetInteractableType() => InteractableType.Item;

    public void SetMaterial()
    {
        Material material = this.GetComponentInChildren<Renderer>().material;
        material.SetTexture(textureProperty, itemSO.Icon.texture);
    }

    public void Clear()
    {
        Destroy(this.gameObject);
    }
}
