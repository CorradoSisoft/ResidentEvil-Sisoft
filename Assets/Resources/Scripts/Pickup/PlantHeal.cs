using UnityEngine;

public class PlantHeal : MonoBehaviour
{
    [Header("Cura")]
    public int healAmount = 20;

    private bool used = false;

    void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null) return;

        // Non raccogliere se la vita è già al massimo
        if (health.currentHealth >= health.maxHealth)
        {
            Debug.Log("Vita già al massimo!");
            return;
        }

        used = true;
        health.Heal(healAmount);

        SaveableObject saveable = GetComponent<SaveableObject>();
        if (saveable != null)
            SaveManager.Instance.RegisterDestroyed(saveable.uniqueID);

        Destroy(gameObject);
    }
}