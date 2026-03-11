using UnityEngine;

public class SafeRoom : MonoBehaviour
{
    [Header("Barriera")]
    public GameObject barrier;

    private bool playerInside = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (playerInside) return;

        playerInside = true;
        BackgroundMusic.Instance.PlaySafeRoom();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        BackgroundMusic.Instance.PlayFloor();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawCube(transform.position, GetComponent<Collider>().bounds.size);
    }
}