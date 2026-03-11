using UnityEngine;
using System.Collections.Generic;

public class SaveableObject : MonoBehaviour
{
    [Tooltip("ID univoco — non cambiarlo mai dopo averlo assegnato!")]
    public string uniqueID;

    void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = System.Guid.NewGuid().ToString();
            return;
        }

        // Controlla duplicati nella scena
        SaveableObject[] all = FindObjectsByType<SaveableObject>(FindObjectsSortMode.None);
        foreach (var other in all)
        {
            if (other != this && other.uniqueID == uniqueID)
            {
                uniqueID = System.Guid.NewGuid().ToString();
                Debug.LogWarning($"ID duplicato rilevato su {gameObject.name} — rigenerato!");
                break;
            }
        }
    }
}