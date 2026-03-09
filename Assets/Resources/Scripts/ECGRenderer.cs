using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways] // Funziona anche fuori dal Play Mode
public class ECGRenderer : MonoBehaviour
{
    [Header("Riferimenti")]
    public RawImage ecgImage;
    public PlayerHealth playerHealth;

    [Header("Dimensioni texture")]
    public int textureWidth = 512;
    public int textureHeight = 64;

    [Header("Aspetto")]
    public Color backgroundColor = Color.black;
    public Color lineColorGreen = Color.green;
    public Color lineColorYellow = Color.yellow;
    public Color lineColorRed = Color.red;
    public int lineThickness = 2;

    [Header("Velocità scorrimento")]
    public float scrollSpeed = 0.5f;

    [Header("Forma ECG")]
    [Range(0.01f, 0.3f)]  public float pWaveHeight = 0.08f;
    [Range(0.1f, 1.5f)]   public float rPeakHeight = 0.75f;  // spike principale
    [Range(0.01f, 0.5f)]  public float sWaveDepth = 0.25f;   // discesa sotto la linea
    [Range(0.01f, 0.3f)]  public float tWaveHeight = 0.12f;
    [Range(0f, 1f)]       public float spikePosition = 0.25f; // posizione del picco nella texture

    [Header("Dinamica per vita")]
    public bool dynamicHeight = true;
    [Range(0f, 1f)] public float minHeightMultiplier = 0.3f; // altezza con vita al minimo

    private Texture2D ecgTexture;
    private float[] ecgCurve;
    private float scrollOffset = 0f;
    private Color currentLineColor;

    // Valori precedenti per rilevare cambiamenti nell'Inspector
    private float prev_pWaveHeight, prev_rPeakHeight, prev_sWaveDepth, prev_tWaveHeight;
    private float prev_spikePosition;
    private int prev_textureWidth, prev_textureHeight, prev_lineThickness;
    private Color prev_backgroundColor, prev_lineColorGreen;

    void OnEnable()
    {
        GenerateECGCurve();
        GenerateTexture();
        CacheInspectorValues();
    }

    void Update()
    {
        // Rileva cambiamenti nell'Inspector e rigenera
        if (InspectorChanged())
        {
            GenerateECGCurve();
            GenerateTexture();
            CacheInspectorValues();
        }

        // Scrolla solo in Play Mode
        if (Application.isPlaying)
        {
            scrollOffset += scrollSpeed * Time.deltaTime;
            if (scrollOffset > 1f) scrollOffset -= 1f;

            if (ecgImage != null)
                ecgImage.uvRect = new Rect(scrollOffset, 0f, 1f, 1f);

            UpdateColorAndHeight();
        }
    }

    void GenerateECGCurve(float heightMultiplier = 1f)
    {
        ecgCurve = new float[textureWidth];

        for (int x = 0; x < textureWidth; x++)
            ecgCurve[x] = 0.5f;

        int spikeStart = Mathf.RoundToInt(spikePosition * textureWidth);
        spikeStart = Mathf.Clamp(spikeStart, 50, textureWidth - 80);

        float hm = heightMultiplier;

        // Onda P
        int pWaveStart = spikeStart - 40;
        for (int x = Mathf.Max(0, pWaveStart); x < Mathf.Min(textureWidth, pWaveStart + 20); x++)
        {
            float t = (float)(x - pWaveStart) / 20f;
            ecgCurve[x] = 0.5f + Mathf.Sin(t * Mathf.PI) * pWaveHeight * hm;
        }

        // Picco Q
        if (spikeStart < textureWidth)     ecgCurve[spikeStart]     = 0.5f - 0.05f * hm;
        if (spikeStart + 1 < textureWidth) ecgCurve[spikeStart + 1] = 0.5f - 0.08f * hm;

        // Picco R
        if (spikeStart + 2 < textureWidth) ecgCurve[spikeStart + 2] = 0.5f + (rPeakHeight - 0.05f) * hm;
        if (spikeStart + 3 < textureWidth) ecgCurve[spikeStart + 3] = 0.5f + rPeakHeight * hm;
        if (spikeStart + 4 < textureWidth) ecgCurve[spikeStart + 4] = 0.5f + (rPeakHeight - 0.05f) * hm;

        // Picco S
        if (spikeStart + 5 < textureWidth) ecgCurve[spikeStart + 5] = 0.5f - sWaveDepth * hm;
        if (spikeStart + 6 < textureWidth) ecgCurve[spikeStart + 6] = 0.5f - (sWaveDepth * 0.8f) * hm;
        if (spikeStart + 7 < textureWidth) ecgCurve[spikeStart + 7] = 0.5f - (sWaveDepth * 0.4f) * hm;
        if (spikeStart + 8 < textureWidth) ecgCurve[spikeStart + 8] = 0.5f;

        // Onda T
        int tWaveStart = spikeStart + 20;
        for (int x = tWaveStart; x < Mathf.Min(textureWidth, tWaveStart + 30); x++)
        {
            float t = (float)(x - tWaveStart) / 30f;
            ecgCurve[x] = 0.5f + Mathf.Sin(t * Mathf.PI) * tWaveHeight * hm;
        }

        // Smoothing
        for (int i = 1; i < textureWidth - 1; i++)
            ecgCurve[i] = (ecgCurve[i - 1] + ecgCurve[i] + ecgCurve[i + 1]) / 3f;
    }

    void GenerateTexture()
    {
        if (ecgImage == null) return;

        ecgTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        ecgTexture.wrapMode = TextureWrapMode.Repeat;

        currentLineColor = lineColorGreen;
        RedrawTexture();
        ecgImage.texture = ecgTexture;
    }

    void RedrawTexture()
    {
        if (ecgTexture == null) return;

        Color[] pixels = new Color[textureWidth * textureHeight];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = backgroundColor;

        for (int x = 0; x < textureWidth; x++)
        {
            int centerY = Mathf.RoundToInt(ecgCurve[x] * (textureHeight - 1));
            centerY = Mathf.Clamp(centerY, 0, textureHeight - 1);

            for (int t = -lineThickness; t <= lineThickness; t++)
            {
                int y = Mathf.Clamp(centerY + t, 0, textureHeight - 1);
                pixels[y * textureWidth + x] = currentLineColor;
            }
        }

        ecgTexture.SetPixels(pixels);
        ecgTexture.Apply();
    }

    void UpdateColorAndHeight()
    {
        if (playerHealth == null) return;

        float ratio = playerHealth.GetRatio();
        Color targetColor;

        if (ratio > PlayerHealth.YELLOW_THRESHOLD)
            targetColor = lineColorGreen;
        else if (ratio > PlayerHealth.RED_THRESHOLD)
            targetColor = lineColorYellow;
        else
            targetColor = lineColorRed;

        // Altezza dinamica: più sei basso di vita, più il battito è debole
        if (dynamicHeight)
        {
            float heightMultiplier = Mathf.Lerp(minHeightMultiplier, 1f, ratio);
            GenerateECGCurve(heightMultiplier);
        }

        if (targetColor != currentLineColor || dynamicHeight)
        {
            currentLineColor = targetColor;
            RedrawTexture();
        }

        // Accelera scrollSpeed con vita bassa
        if (ratio <= PlayerHealth.RED_THRESHOLD)
            scrollSpeed = 1.2f;
        else if (ratio <= PlayerHealth.YELLOW_THRESHOLD)
            scrollSpeed = 0.8f;
        else
            scrollSpeed = 0.5f;
    }

    // --- Inspector change detection ---

    bool InspectorChanged()
    {
        return prev_pWaveHeight != pWaveHeight
            || prev_rPeakHeight != rPeakHeight
            || prev_sWaveDepth != sWaveDepth
            || prev_tWaveHeight != tWaveHeight
            || prev_spikePosition != spikePosition
            || prev_textureWidth != textureWidth
            || prev_textureHeight != textureHeight
            || prev_lineThickness != lineThickness
            || prev_backgroundColor != backgroundColor
            || prev_lineColorGreen != lineColorGreen;
    }

    void CacheInspectorValues()
    {
        prev_pWaveHeight   = pWaveHeight;
        prev_rPeakHeight   = rPeakHeight;
        prev_sWaveDepth    = sWaveDepth;
        prev_tWaveHeight   = tWaveHeight;
        prev_spikePosition = spikePosition;
        prev_textureWidth  = textureWidth;
        prev_textureHeight = textureHeight;
        prev_lineThickness = lineThickness;
        prev_backgroundColor = backgroundColor;
        prev_lineColorGreen  = lineColorGreen;
    }
}