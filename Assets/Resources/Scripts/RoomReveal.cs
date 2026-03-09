using UnityEngine;

public class RoomReveal : MonoBehaviour
{
    public string roomID;  // Es: "room_01", "room_02"...

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            MapManager.Instance.RevealRoom(roomID);
    }
}