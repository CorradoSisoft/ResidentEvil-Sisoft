using UnityEngine;
using UnityEngine.UI;

public class SafeLockUI : MonoBehaviour
{
    public Button[] numberButtons;
    public Canvas targetCanvas; // ← trascina il MainCanvas qui

    private Camera canvasCamera;

    void Start()
    {
        canvasCamera = targetCanvas != null ? targetCanvas.worldCamera : null;
        Debug.Log($"Camera: {canvasCamera}");

        /* for (int i = 0; i < numberButtons.Length; i++)
        {
            int captured = i;
            numberButtons[i].onClick.AddListener(() =>
                SafeLock.Current?.PressKey(captured.ToString()));
        } */
    }

    void Update()
    {
        if (!SafeLock.IsAnyOpen) return;

        if (Input.GetMouseButtonDown(0))
        {
            for (int i = 0; i < numberButtons.Length; i++)
            {
                if (numberButtons[i] == null) continue;
                int captured = i;
                if (RectTransformUtility.RectangleContainsScreenPoint(
                    numberButtons[i].GetComponent<RectTransform>(), Input.mousePosition, canvasCamera))
                {
                    SafeLock.Current?.PressKey(captured.ToString());
                    return;
                }
            }
        }
    }
}