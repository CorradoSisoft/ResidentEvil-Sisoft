using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vita")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Test Danno")]
    public int testDamageAmount = 10;

    // Soglie (usate anche da ECGRenderer)
    public const float YELLOW_THRESHOLD = 0.6f;
    public const float RED_THRESHOLD = 0.3f;

    // Evento per notificare altri script (ECGRenderer, UI, ecc.)
    public event System.Action OnHealthChanged;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke();
        if (currentHealth <= 0) Die();
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

    public float GetRatio() => (float)currentHealth / maxHealth;

    void Die()
    {
        Debug.Log("Player morto!");
    }
}