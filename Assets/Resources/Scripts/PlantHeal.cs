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

        used = true;
        health.Heal(healAmount);
        Destroy(gameObject);
    }
}