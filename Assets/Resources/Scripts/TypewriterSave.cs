using UnityEngine;

public class TypewriterSave : MonoBehaviour, IInteragibile
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip saveSound; // suono classico della macchinetta

    public void Interagisci()
    {
        SaveManager.Instance.Save();

        if (audioSource != null && saveSound != null)
            audioSource.PlayOneShot(saveSound);

        Debug.Log("Partita salvata!");
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