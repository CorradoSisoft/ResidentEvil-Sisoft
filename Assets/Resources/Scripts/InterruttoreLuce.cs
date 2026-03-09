using UnityEngine;

public class InterruttoreLuce : MonoBehaviour, IInteragibile
{
    [Header("Impostazioni")]
    public Light[] luci;
    public bool funzionante = true;

    private bool accesa = true;

    public void Interagisci()
    {
        if (!funzionante) return;

        accesa = !accesa;
        foreach (Light luce in luci)
            if (luce != null)
                luce.enabled = accesa;
    }

    public void MostraHint(GameObject hintInteract, GameObject hintNonFunziona, GameObject hintChiave)
    {
        if (!funzionante)
        {
            if (hintNonFunziona != null) hintNonFunziona.SetActive(true);
        }
        else
        {
            if (hintInteract != null) hintInteract.SetActive(true);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = funzionante ? Color.yellow : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 3f);
    }
}