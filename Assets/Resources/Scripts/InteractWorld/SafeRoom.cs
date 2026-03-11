using UnityEngine;

public class SafeRoom : MonoBehaviour
{
    [Header("Musica")]
    public AudioSource musicSource;
    public AudioClip safeRoomMusic;

    [Header("Barriera")]
    public GameObject barrier; // il box collider invisibile

    private bool playerInside = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (playerInside) return;

        playerInside = true;

        // Musica
        if (musicSource != null && safeRoomMusic != null)
        {
            musicSource.clip = safeRoomMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        // Ferma musica quando esce
        if (musicSource != null)
            musicSource.Stop();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawCube(transform.position, GetComponent<Collider>().bounds.size);
    }
}