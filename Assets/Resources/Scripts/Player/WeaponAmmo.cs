using UnityEngine;
using TMPro;

public class WeaponAmmo : MonoBehaviour
{
    public int magazineSize = 6;
    public int ammoInMagazine;
    public int startingAmmoInMagazine = 0; // ← separato
    public int totalAmmo = 24;

    public TextMeshProUGUI ammoInfo;

    void Start()
    {
        ammoInMagazine = startingAmmoInMagazine;
        UpdateAmmoUI();
    }

    public bool CanShoot()
    {
        return ammoInMagazine > 0;
    }

    public void Shoot()
    {
        ammoInMagazine--;
        UpdateAmmoUI();
    }

    public void Reload()
    {
        Debug.Log($"Reload chiamato | inMag: {ammoInMagazine} | total: {totalAmmo}");
        if (ammoInMagazine == magazineSize) return;
        if (totalAmmo <= 0) return;

        int ammoNeeded = magazineSize - ammoInMagazine;
        int ammoToLoad = Mathf.Min(ammoNeeded, totalAmmo);

        ammoInMagazine += ammoToLoad;
        totalAmmo -= ammoToLoad;

        UpdateAmmoUI();
    }

    public void UpdateAmmoUI()
    {
        ammoInfo.text = ammoInMagazine + " / " + totalAmmo;
    }
}