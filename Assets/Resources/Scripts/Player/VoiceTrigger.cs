using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class VoiceTrigger : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip voiceClip;

    [Header("Sottotitoli")]
    public TextMeshProUGUI subtitleText;
    public string subtitleString = "Che ci fai qui?";
    public float subtitleDuration = 3f;

    public bool triggered = false;

    void Start()
    {
        // Assicura che il collider sia trigger
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // Registra come distrutto nel SaveManager
        SaveableObject saveable = GetComponent<SaveableObject>();
        if (saveable != null)
            SaveManager.Instance.RegisterDestroyed(saveable.uniqueID);

        PlayVoice();
    }

    void PlayVoice()
    {
        if (audioSource != null && voiceClip != null)
            audioSource.PlayOneShot(voiceClip);

        if (subtitleText != null && !string.IsNullOrEmpty(subtitleString))
            StartCoroutine(ShowSubtitle());
    }

    private System.Collections.IEnumerator ShowSubtitle()
    {
        subtitleText.text = subtitleString;
        subtitleText.gameObject.SetActive(true);
        yield return new WaitForSeconds(subtitleDuration);
        subtitleText.gameObject.SetActive(false);
        subtitleText.text = "";
    }

    // Chiamato dal SaveManager al load — non rifar partire l'audio
    public void SetTriggered()
    {
        triggered = true;
    }
}