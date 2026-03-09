using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vita")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Test Danno")]
    public int testDamageAmount = 10;

    [Header("Damage Feedback")]
    public UnityEngine.UI.Image damageOverlay; // Image rossa, alpha 0 di default
    public float flashDuration = 0.3f;

    // Soglie (usate anche da ECGRenderer)
    public const float YELLOW_THRESHOLD = 0.6f;
    public const float RED_THRESHOLD = 0.3f;

    // Evento per notificare altri script (ECGRenderer, UI, ecc.)
    public event System.Action OnHealthChanged;

    private Coroutine flashCoroutine;



    void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke();
    }

    [ContextMenu("Test TakeDamage")]
    public void TestTakeDamage()
    {
        TakeDamage(testDamageAmount);
    }

        public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke();
        
        // Flash rosso
        if (damageOverlay != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0) Die();
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        // Appare subito
        damageOverlay.color = new Color(1, 0, 0, 0.4f);

        // Svanisce gradualmente
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.4f, 0f, elapsed / flashDuration);
            damageOverlay.color = new Color(1, 0, 0, alpha);
            yield return null;
        }

        damageOverlay.color = new Color(1, 0, 0, 0f);
    }

    public float GetRatio() => (float)currentHealth / maxHealth;

    void Die()
    {
        Debug.Log("Player morto!");
    }
}