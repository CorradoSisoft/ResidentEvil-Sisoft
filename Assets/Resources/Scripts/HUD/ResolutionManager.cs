using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    private const float TARGET_WIDTH = 800f;
    private const float TARGET_HEIGHT = 600f;

    public Camera[] cameras;

    void Awake()
    {
        ApplyAspect();
    }

    void Update()
    {
        ApplyAspect(); // aggiorna se la finestra viene ridimensionata

        if (Input.GetKeyDown(KeyCode.Return) && Input.GetKey(KeyCode.LeftAlt))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }

    void ApplyAspect()
    {
        float targetAspect = TARGET_WIDTH / TARGET_HEIGHT;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect;

        if (scaleHeight < 1.0f)
        {
            rect = new Rect(0, (1f - scaleHeight) / 2f, 1f, scaleHeight);
        }
        else
        {
            float scaleWidth = 1f / scaleHeight;
            rect = new Rect((1f - scaleWidth) / 2f, 0, scaleWidth, 1f);
        }

        foreach (Camera cam in cameras)
            if (cam != null) cam.rect = rect;
    }
}