using UnityEngine;

public class PickupItem : MonoBehaviour, IInteragibile
{
    public ItemData itemData;
    public float interactionDistance = 2f;

    public void Interagisci()
    {
        if (InventoryManager.Instance.AddItem(itemData))
        {
            if (itemData.itemType == ItemType.SpecialDocument)
                DocumentCounter.Instance.DocumentFound();

            Debug.Log($"Raccolto: {itemData.itemName}");
            SaveableObject saveable = GetComponent<SaveableObject>();
            if (saveable != null)
                SaveManager.Instance.RegisterDestroyed(saveable.uniqueID);

            // Apre l'inventario automaticamente
            InventoryManager.Instance.OpenInventory();

            Destroy(gameObject);
        }
    }

    public void MostraHint(GameObject hintInteract, GameObject hintNonFunziona, GameObject hintChiave)
    {
        if (hintInteract != null) hintInteract.SetActive(true);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}