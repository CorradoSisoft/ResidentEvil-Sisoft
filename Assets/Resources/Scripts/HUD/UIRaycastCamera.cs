using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // ← mancava questo

public class UIRaycastCamera : MonoBehaviour
{
    public Camera hudCamera;

    void Start()
    {
        GetComponent<EventSystem>().pixelDragThreshold = 10;
        // Forza la camera per il raycasting UI
        foreach (var raycaster in FindObjectsOfType<GraphicRaycaster>())
        {
            var canvas = raycaster.GetComponent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
                canvas.worldCamera = hudCamera;
        }
    }
}