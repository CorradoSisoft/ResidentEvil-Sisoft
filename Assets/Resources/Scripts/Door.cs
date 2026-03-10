using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour, IInteragibile
{
    [Header("Impostazioni")]
    public bool requiresKey = false;
    public string requiredKeyId; // deve corrispondere a ItemData.keyId
    public float openAngle = 90f;
    public float openSpeed = 0.5f;

    private bool isOpen = false;
    private bool isAnimating = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;      // suono porta
    public AudioClip voiceSound;     // voce personaggio

    public void Interagisci()
    {
        if (isOpen || isAnimating) return;

        if (requiresKey)
        {
            ItemData key = InventoryManager.Instance.GetKeyById(requiredKeyId);
            if (key == null)
            {
                Debug.Log("Serve una chiave!");
                return;
            }

            InventoryManager.Instance.RemoveItem(key);
            Debug.Log("Porta aperta!");

            if (audioSource != null)
            {
                if (voiceSound != null) audioSource.PlayOneShot(voiceSound);
                if (openSound != null) audioSource.PlayOneShot(openSound);
            }
        }

        StartCoroutine(OpenDoor());
    }

    public void MostraHint(GameObject hintInteract, GameObject hintNonFunziona, GameObject hintChiave)
    {
        if (isOpen) return;

        if (requiresKey)
        {
            bool hasKey = InventoryManager.Instance.GetKeyById(requiredKeyId) != null;
            if (hasKey)
            {
                if (hintInteract != null) hintInteract.SetActive(true); // "Premi E"
            }
            else
            {
                if (hintChiave != null) hintChiave.SetActive(true); // "Serve una chiave"
            }
        }
        else
        {
            if (hintInteract != null) hintInteract.SetActive(true);
        }
    }

    private IEnumerator OpenDoor()
    {
        isAnimating = true;

        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
            yield return new WaitForSeconds(0.01f);
            meshRenderer.enabled = true;
        }

        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime * openSpeed;
            float t = Mathf.SmoothStep(0, 1, time);
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        isOpen = true;
        isAnimating = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = requiresKey ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 3f);
    }
}