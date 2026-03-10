using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlantHeal : MonoBehaviour
{
    [Header("Cura")]
    public int healAmount = 20;

    [Header("Feedback")]
    public Image healOverlay; // Image verde a schermo intero
    public float flashDuration = 0.5f;

    private bool used = false;

    void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null) return;

        used = true;
        health.Heal(healAmount);

        if (healOverlay != null)
            StartCoroutine(HealFlash());

        // Distruggi la pianta dopo l'uso
        Destroy(gameObject, 0.1f);
    }

    private IEnumerator HealFlash()
    {
        healOverlay.color = new Color(0, 1, 0, 0.4f);
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.4f, 0f, elapsed / flashDuration);
            healOverlay.color = new Color(0, 1, 0, alpha);
            yield return null;
        }
        healOverlay.color = new Color(0, 1, 0, 0f);
    }
}