using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterSave : MonoBehaviour, IInteragibile
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip saveSound;

    [Header("Feedback UI")]
    public GameObject saveNotification;
    public float notificationDuration = 3f;

    private Coroutine hideCoroutine;

    public void Interagisci()
    {
        SaveManager.Instance.Save();

        if (audioSource != null && saveSound != null)
            audioSource.PlayOneShot(saveSound);

        if (saveNotification != null)
        {
            saveNotification.SetActive(true);
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HideAfterDelay());
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);
        saveNotification.SetActive(false);
    }

    public void MostraHint(GameObject hintInteract, GameObject hintNonFunziona, GameObject hintChiave)
    {
        if (hintInteract != null) hintInteract.SetActive(true);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 3f);
    }
}