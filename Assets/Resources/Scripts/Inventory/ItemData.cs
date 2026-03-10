using UnityEngine;

public enum ItemType { Generic, Key }

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea(3, 8)] public string description;
    public ItemType itemType = ItemType.Generic;
    public string keyId; // es. "key_room1" — deve corrispondere alla porta

    void OnValidate()
    {
        // Rimuove spazi iniziali e finali automaticamente
        itemName = itemName.Trim();
        keyId = keyId.Trim();
    }
}