using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Crea e gestisce la crosshair FPS via codice.
/// Aggiungi questo script su un qualsiasi GameObject in scena (es. il Player).
/// Non serve configurare nulla nell'Inspector.
/// </summary>
public class FPCrosshair : MonoBehaviour
{
    public static FPCrosshair Instance { get; private set; }

    [Header("Crosshair Settings")]
    public Color crosshairColor = new Color(1f, 1f, 1f, 0.85f);
    public float crosshairSize = 12f;   // lunghezza di ogni braccio in pixel
    public float crosshairThickness = 2f;
    public float crosshairGap = 4f;     // spazio vuoto al centro

    private Canvas canvas;
    private GameObject crosshairRoot;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BuildCrosshair();
        SetVisible(false); // parte nascosta, si attiva con il toggle camera
    }

    void BuildCrosshair()
    {
        // Crea Canvas screen-space overlay
        GameObject canvasGO = new GameObject("FPCrosshairCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);

        // Root della crosshair (centro schermo)
        crosshairRoot = new GameObject("Crosshair");
        crosshairRoot.transform.SetParent(canvasGO.transform, false);
        RectTransform rootRect = crosshairRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = Vector2.zero;

        // Crea i 4 bracci: su, giù, sinistra, destra
        CreateBar("Up",    new Vector2(0f,  crosshairGap + crosshairSize * 0.5f), new Vector2(crosshairThickness, crosshairSize));
        CreateBar("Down",  new Vector2(0f, -(crosshairGap + crosshairSize * 0.5f)), new Vector2(crosshairThickness, crosshairSize));
        CreateBar("Left",  new Vector2(-(crosshairGap + crosshairSize * 0.5f), 0f), new Vector2(crosshairSize, crosshairThickness));
        CreateBar("Right", new Vector2( crosshairGap + crosshairSize * 0.5f,  0f), new Vector2(crosshairSize, crosshairThickness));

        // Puntino centrale opzionale
        CreateDot();
    }

    void CreateBar(string barName, Vector2 anchoredPos, Vector2 size)
    {
        GameObject bar = new GameObject(barName);
        bar.transform.SetParent(crosshairRoot.transform, false);

        RectTransform rt = bar.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = bar.AddComponent<Image>();
        img.color = crosshairColor;
    }

    void CreateDot()
    {
        GameObject dot = new GameObject("Dot");
        dot.transform.SetParent(crosshairRoot.transform, false);

        RectTransform rt = dot.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(crosshairThickness, crosshairThickness);

        Image img = dot.AddComponent<Image>();
        img.color = crosshairColor;
    }

    public void SetVisible(bool visible)
    {
        if (crosshairRoot != null)
            crosshairRoot.SetActive(visible);
    }
}
