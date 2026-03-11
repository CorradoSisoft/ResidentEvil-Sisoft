using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // Player
    public float playerX, playerY, playerZ;
    public float playerRotY;
    public int playerHealth;
    public int ammoInMagazine;
    public int totalAmmo;

    // Inventario
    public List<string> inventoryItemNames = new List<string>();

    // Stato mondo
    public List<string> destroyedObjects = new List<string>(); // barre, piante, munizioni
    public List<string> deadZombies      = new List<string>();
    public List<string> openDoors        = new List<string>();
    public List<string> visitedRooms     = new List<string>();

    // Flag GameManager
    public List<string> gameFlags = new List<string>();

    public int documentsFound;

    // Casseforti risolte
    public List<string> solvedSafes = new List<string>();
}