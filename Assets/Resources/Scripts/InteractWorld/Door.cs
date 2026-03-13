using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour, IInteragibile
{
    [Header("Impostazioni")]
    public bool requiresKey = false;
    public string requiredKeyId;
    public float openAngle = 90f;
    public float openSpeed = 0.5f;

    private bool isOpen = false;
    private bool isAnimating = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip voiceSound;

    public bool IsOpen => isOpen;

    [Header("EndGame")]
    public bool isEndGameDoor = false;
    public GameObject endGamePanel;
    public string hintDocumentiMancanti = "Trova tutti i documenti";

    [Header("Hints")]
    public string hintChiaveMancante = "Serve una chiave!";

    public void OpenInstant()
    {
        isOpen = true;
        transform.rotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    public void Interagisci()
    {
        if (isEndGameDoor)
        {
            if (!DocumentCounter.Instance.IsEndingUnlocked)
            {
                Debug.Log("Devi trovare tutti i documenti!");
                return;
            }

            if (endGamePanel != null)
            {
                EndGameSequence.Instance.StartEnding();
            }
            return;
        }

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
        if (isEndGameDoor)
        {
            if (DocumentCounter.Instance.IsEndingUnlocked)
            {
                if (hintInteract != null) hintInteract.SetActive(true);
            }
            else
            {
                if (hintNonFunziona != null)
                {
                    var tmp = hintNonFunziona.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (tmp != null) tmp.text = hintDocumentiMancanti;
                    hintNonFunziona.SetActive(true);
                }
            }
            return;
        }

        if (isOpen || isAnimating) return;

        if (requiresKey)
        {
            bool hasKey = InventoryManager.Instance.GetKeyById(requiredKeyId) != null;
            if (hasKey)
            {
                if (hintInteract != null) hintInteract.SetActive(true);
            }
            else
            {
                if (hintNonFunziona != null)
                {
                    var tmp = hintNonFunziona.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (tmp != null) tmp.text = hintChiaveMancante;
                    hintNonFunziona.SetActive(true);
                }
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