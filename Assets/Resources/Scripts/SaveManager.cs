using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string savePath => Application.persistentDataPath + "/save.json";
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ─── SAVE ───────────────────────────────────────────
    public void Save()
    {
        SaveData data = new SaveData();

        // Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerX = player.transform.position.x;
            data.playerY = player.transform.position.y;
            data.playerZ = player.transform.position.z;
            data.playerRotY = player.transform.eulerAngles.y;

            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null) data.playerHealth = ph.currentHealth;

            WeaponAmmo wa = player.GetComponentInChildren<WeaponAmmo>();
            if (wa != null)
            {
                data.ammoInMagazine = wa.ammoInMagazine;
                data.totalAmmo      = wa.totalAmmo;
            }
        }

        // Inventario
        foreach (var item in InventoryManager.Instance.GetAllItems())
            if (item != null) data.inventoryItemNames.Add(item.itemName);

        // Zombie morti
        foreach (var zombie in FindObjectsOfType<ZombieController>())
            if (zombie.IsDead)
                data.deadZombies.Add(zombie.GetComponent<SaveableObject>()?.uniqueID);

        // Porte aperte
        foreach (var door in FindObjectsOfType<Door>())
            if (door.IsOpen)
                data.openDoors.Add(door.GetComponent<SaveableObject>()?.uniqueID);

        // Oggetti distrutti
        // (salvati dinamicamente quando vengono distrutti — vedi sotto)
        data.destroyedObjects = _destroyedObjects;

        // Stanze visitate
        data.visitedRooms = MapManager.Instance.GetVisitedRooms();

        data.documentsFound = DocumentCounter.Instance.GetFound();

        // Flag
        data.gameFlags = GameManager.Instance.GetAllFlags();

        // Casseforti risolte
        foreach (var safe in FindObjectsOfType<SafeLock>())
        {
            if (safe.IsSolved)
            {
                string id = safe.GetComponent<SaveableObject>()?.uniqueID;
                if (id != null) data.solvedSafes.Add(id);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"[SaveManager] Salvato in: {savePath}");
    }

    // ─── LOAD ───────────────────────────────────────────
    public void Load()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("[SaveManager] Nessun salvataggio trovato.");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(data.playerX, data.playerY, data.playerZ);
            player.transform.rotation = Quaternion.Euler(0, data.playerRotY, 0);

            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.currentHealth = data.playerHealth;
                ph.NotifyHealthChanged();
            }

            WeaponAmmo wa = player.GetComponentInChildren<WeaponAmmo>();
            if (wa != null)
            {
                wa.ammoInMagazine = data.ammoInMagazine;
                wa.totalAmmo      = data.totalAmmo;
                wa.UpdateAmmoUI();
            }
        }

        // Inventario
        InventoryManager.Instance.ClearInventory();
        foreach (var itemName in data.inventoryItemNames)
        {
            Debug.Log($"Cerco item: '{itemName}'");
            ItemData item = Resources.Load<ItemData>($"Items/{itemName.Trim()}");
            if (item != null) InventoryManager.Instance.AddItem(item);
        }

        // Zombie morti
        foreach (var zombie in FindObjectsOfType<ZombieController>())
        {
            string id = zombie.GetComponent<SaveableObject>()?.uniqueID;
            if (id != null && data.deadZombies.Contains(id))
                zombie.DieInstant();
        }

        // Porte aperte
        foreach (var door in FindObjectsOfType<Door>())
        {
            string id = door.GetComponent<SaveableObject>()?.uniqueID;
            if (id != null && data.openDoors.Contains(id))
                door.OpenInstant();
        }

        // Oggetti distrutti
        _destroyedObjects = data.destroyedObjects;
        foreach (var obj in FindObjectsOfType<SaveableObject>())
        {
            if (_destroyedObjects.Contains(obj.uniqueID))
            {
                // Se è un VoiceTrigger, non distruggerlo — solo marcalo come già triggerato
                VoiceTrigger vt = obj.GetComponent<VoiceTrigger>();
                if (vt != null)
                {
                    vt.SetTriggered();
                    continue; // ← salta il Destroy
                }

                Destroy(obj.gameObject);
            }
        }

        // Stanze visitate
        MapManager.Instance.RestoreVisitedRooms(data.visitedRooms);

        DocumentCounter.Instance.RestoreCount(data.documentsFound);

        // Flag
        GameManager.Instance.RestoreFlags(data.gameFlags);

        // Casseforti risolte
        foreach (var safe in FindObjectsOfType<SafeLock>())
        {
            string id = safe.GetComponent<SaveableObject>()?.uniqueID;
            if (id != null && data.solvedSafes.Contains(id))
                safe.SetSolved();
        }

        Debug.Log("[SaveManager] Caricamento completato!");
    }

    public bool SaveExists() => File.Exists(savePath);

    public void DeleteSave()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);
    }

    // ─── REGISTRO OGGETTI DISTRUTTI ─────────────────────
    private List<string> _destroyedObjects = new List<string>();

    public void RegisterDestroyed(string uniqueID)
    {
        if (!_destroyedObjects.Contains(uniqueID))
            _destroyedObjects.Add(uniqueID);
    }
}