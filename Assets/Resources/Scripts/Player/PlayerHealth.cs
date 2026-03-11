using UnityEngine;
using UnityEngine.UI;
using System.Collections; // ← mancava questo

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

    [Header("Heal Feedback")]
    public UnityEngine.UI.Image healOverlay;
    public float healFlashDuration = 0.5f;

    [Header("Portraits")]
    public Image portrait;            // L'Image UI del portrait
    public Sprite[] healthPortraits;  // 0 = verde, 1 = giallo, 2 = rosso

    [Header("Voice")]
    public AudioSource voiceAudio;
    public AudioClip[] damageSounds;
    public AudioClip[] healSounds;
    
    // Soglie (usate anche da ECGRenderer)
    public const float YELLOW_THRESHOLD = 0.6f;
    public const float RED_THRESHOLD = 0.3f;

    // Evento per notificare altri script (ECGRenderer, UI, ecc.)
    public event System.Action OnHealthChanged;

    private Coroutine flashCoroutine;
    private Coroutine healCoroutine;


    void Start()
    {
        currentHealth = maxHealth;
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
        UpdatePortrait();

        // Voce danno
        if (voiceAudio != null && damageSounds.Length > 0)
        {
            AudioClip clip = damageSounds[Random.Range(0, damageSounds.Length)];
            voiceAudio.PlayOneShot(clip);
        }

        if (damageOverlay != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0) Die();
    }

    private IEnumerator DamageFlash()
    {
        damageOverlay.color = new Color(1, 0, 0, 0.4f);
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

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke();
        UpdatePortrait();

        // Voce cura — era finita in DamageFlash per errore
        if (voiceAudio != null && healSounds.Length > 0)
        {
            AudioClip clip = healSounds[Random.Range(0, healSounds.Length)];
            voiceAudio.PlayOneShot(clip);
        }

        if (healOverlay != null)
        {
            if (healCoroutine != null) StopCoroutine(healCoroutine);
            healCoroutine = StartCoroutine(HealFlash());
        }
    }

    private IEnumerator HealFlash()
    {
        healOverlay.color = new Color(0, 1, 0, 0.4f);
        float elapsed = 0f;
        while (elapsed < healFlashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.4f, 0f, elapsed / healFlashDuration);
            healOverlay.color = new Color(0, 1, 0, alpha);
            yield return null;
        }
        healOverlay.color = new Color(0, 1, 0, 0f);
    }

    void UpdatePortrait()
    {
        float ratio = GetRatio();

        if (ratio > YELLOW_THRESHOLD)        // Verde
            portrait.sprite = healthPortraits[0];
        else if (ratio > RED_THRESHOLD)      // Giallo
            portrait.sprite = healthPortraits[1];
        else                                 // Rosso
            portrait.sprite = healthPortraits[2];
    }

    public float GetRatio() => (float)currentHealth / maxHealth;

    void Die()
    {
        GetComponent<PlayerMovement>().OnPlayerDeath();
        StartCoroutine(DelayedGameOver());
    }

    private IEnumerator DelayedGameOver()
    {
        yield return new WaitForSecondsRealtime(2f);
        GameOverManager.Instance.ShowGameOver();
    }

    public void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke();
    }
}