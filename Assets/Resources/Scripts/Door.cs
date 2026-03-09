using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour, IInteragibile
{
    [Header("Impostazioni")]
    public bool requiresKey = false;
    public float openAngle = 90f;
    public float openSpeed = 0.5f;

    private bool isOpen = false;
    private bool isAnimating = false;

    public void Interagisci()
    {
        if (isOpen || isAnimating) return;

        if (requiresKey)
        {
            Debug.Log("Serve una chiave!");
            return;
        }

        StartCoroutine(OpenDoor());
    }

    public void MostraHint(GameObject hintInteract, GameObject hintNonFunziona, GameObject hintChiave)
    {
        if (isOpen) return;

        if (requiresKey)
        {
            if (hintChiave != null) hintChiave.SetActive(true);
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