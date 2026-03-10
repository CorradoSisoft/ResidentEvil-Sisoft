using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Munizioni")]
    public int ammoAmount = 12;

    private bool collected = false;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        WeaponAmmo weapon = other.GetComponentInChildren<WeaponAmmo>();
        if (weapon == null)
            weapon = other.GetComponentInParent<WeaponAmmo>();
        if (weapon == null) return;

        collected = true;
        weapon.totalAmmo += ammoAmount;
        weapon.UpdateAmmoUI();

        Debug.Log($"Raccolte {ammoAmount} munizioni!");
        Destroy(gameObject);
    }
}