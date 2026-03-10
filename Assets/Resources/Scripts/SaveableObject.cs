using UnityEngine;

public class SaveableObject : MonoBehaviour
{
    [Tooltip("ID univoco — non cambiarlo mai dopo averlo assegnato!")]
    public string uniqueID;

    void OnValidate()
    {
        // Genera ID automatico se vuoto
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = System.Guid.NewGuid().ToString();
    }
}