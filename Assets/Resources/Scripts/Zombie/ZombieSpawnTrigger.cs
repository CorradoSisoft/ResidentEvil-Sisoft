using UnityEngine;

public class ZombieSpawnTrigger : MonoBehaviour
{
    public GameObject[] zombiePrefabs;
    public Transform[] spawnPoints;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        Destroy(gameObject); // ← sostituisce gameObject.SetActive(false)

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (i < zombiePrefabs.Length)
                Instantiate(zombiePrefabs[i], spawnPoints[i].position, spawnPoints[i].rotation);
        }

        SaveableObject saveable = GetComponent<SaveableObject>();
        if (saveable != null)
            SaveManager.Instance.RegisterDestroyed(saveable.uniqueID);
    }
}