using UnityEngine;
using System.Collections;

public class InteractionManager : MonoBehaviour
{
    [Header("Interazione")]
    public float interactionDistance = 3f;
    public KeyCode interactionKey = KeyCode.E;

    [Header("UI Hint")]
    public GameObject hintInteract;      // "Premi E"
    public GameObject hintNonFunziona;   // "Non funziona"
    public GameObject hintChiave;        // "Serve una chiave"

    void Update()
    {
        HideAllHints();

        // Trova l'interagibile più vicino
        IInteragibile closest = FindClosest();
        if (closest == null) return;

        // Mostra hint appropriato
        closest.MostraHint(hintInteract, hintNonFunziona, hintChiave);

        // Interagisci con E
        if (Input.GetKeyDown(interactionKey))
            closest.Interagisci();
    }

    IInteragibile FindClosest()
    {
        IInteragibile closest = null;
        float closestDist = float.MaxValue;

        foreach (var obj in FindObjectsOfType<MonoBehaviour>())
        {
            if (obj is IInteragibile interagibile)
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist <= interactionDistance && dist < closestDist)
                {
                    closestDist = dist;
                    closest = interagibile;
                }
            }
        }

        return closest;
    }

    void HideAllHints()
    {
        if (hintInteract != null)    hintInteract.SetActive(false);
        if (hintNonFunziona != null) hintNonFunziona.SetActive(false);
        if (hintChiave != null)      hintChiave.SetActive(false);
    }
}