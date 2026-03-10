using UnityEngine;
using TMPro;

public class WeaponAmmo : MonoBehaviour
{
    public int magazineSize = 6;
    public int ammoInMagazine;
    public int totalAmmo = 24;

    public TextMeshProUGUI ammoInfo;

    void Start()
    {
        ammoInMagazine = magazineSize;
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