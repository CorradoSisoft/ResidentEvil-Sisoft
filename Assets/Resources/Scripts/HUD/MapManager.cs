using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [System.Serializable]
    public class RoomOverlay
    {
        public string roomID;
        public Image overlayImage;  // L'overlay UI di questa stanza
    }

    public List<RoomOverlay> rooms = new List<RoomOverlay>();

    public Color unvisitedColor = new Color(1f, 0f, 0f, 0.5f);  // Rosso semitrasparente
    public Color visitedColor = new Color(0f, 0.5f, 1f, 0.3f);  // Blu semitrasparente

    private HashSet<string> visitedRooms = new HashSet<string>();
    public List<string> GetVisitedRooms() => new List<string>(visitedRooms);

    public void RestoreVisitedRooms(List<string> rooms)
    {
        foreach (var id in rooms)
            RevealRoom(id);
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Inizializza tutti gli overlay come rossi
        foreach (var room in rooms)
        {
            if (room.overlayImage != null)
                room.overlayImage.color = unvisitedColor;
        }
    }

    public void RevealRoom(string roomID)
    {
        if (visitedRooms.Contains(roomID)) return; // Già visitata

        visitedRooms.Add(roomID);

        var room = rooms.Find(r => r.roomID == roomID);
        if (room != null && room.overlayImage != null)
            room.overlayImage.color = visitedColor;
    }
}
