using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// GTA Vice City Stories – Day/Night Cycle
/// Replica lo stile visivo di VCS: alba arancio/rosa, giorno soleggiato,
/// tramonto viola/magenta, notte blu scuro con accenni di neon.
///
/// SETUP:
///  1. Aggiungi questo script a un GameObject vuoto (es. "DayNightManager")
///  2. Trascina la Directional Light nel campo "sunLight"
///  3. (Opzionale) Trascina una seconda Directional Light debole nel campo "moonLight"
///  4. Assegna il Gradient "skyColor" e "fogColor" direttamente nell'Inspector
///  5. Per i neon notturni: aggiungi Point/Spot Lights nella lista "neonLights"
/// </summary>
[ExecuteAlways]
public class DayNightCycle : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // TEMPO
    // ─────────────────────────────────────────────
    [Header("── Tempo ──────────────────────────────")]
    [Tooltip("Ora corrente (0 = mezzanotte, 12 = mezzogiorno, 24 = mezzanotte)")]
    [Range(0f, 24f)]
    public float currentHour = 8f;

    [Tooltip("Durata di un giorno completo in secondi reali (es. 600 = 10 minuti)")]
    public float dayDurationSeconds = 600f;

    [Tooltip("Se false, il tempo si ferma (utile per debug)")]
    public bool timeRunning = true;

    // ─────────────────────────────────────────────
    // LUCI
    // ─────────────────────────────────────────────
    [Header("── Luci ────────────────────────────────")]
    public Light sunLight;
    public Light moonLight;

    [Tooltip("Intensità massima del sole a mezzogiorno")]
    [Range(0f, 5f)]
    public float sunMaxIntensity = 1.4f;

    [Tooltip("Intensità massima della luna di notte")]
    [Range(0f, 1f)]
    public float moonMaxIntensity = 0.15f;

    // ─────────────────────────────────────────────
    // COLORI SOLE – stile VCS
    // ─────────────────────────────────────────────
    [Header("── Colori Sole (stile VCS) ─────────────")]
    [Tooltip("Gradiente del colore del sole nelle 24 ore (x = ora normalizzata 0-1)")]
    public Gradient sunColorGradient;

    // ─────────────────────────────────────────────
    // AMBIENTE (Sky + Fog)
    // ─────────────────────────────────────────────
    [Header("── Ambiente ────────────────────────────")]
    public Gradient skyColorGradient;
    public Gradient fogColorGradient;

    [Tooltip("Intensità luce ambientale minima (notte)")]
    [Range(0f, 1f)]
    public float ambientNight = 0.05f;

    [Tooltip("Intensità luce ambientale massima (giorno)")]
    [Range(0f, 1f)]
    public float ambientDay = 0.8f;

    [Tooltip("Attiva/disattiva il fog dinamico")]
    public bool dynamicFog = true;

    [Tooltip("Densità fog massima (notte / golden hour)")]
    [Range(0f, 0.1f)]
    public float fogDensityMax = 0.018f;

    // ─────────────────────────────────────────────
    // NEON NOTTURNI
    // ─────────────────────────────────────────────
    [Header("── Neon Notturni ───────────────────────")]
    [Tooltip("Luci neon che si accendono di notte (Point/Spot Lights)")]
    public Light[] neonLights;

    [Tooltip("Ora in cui i neon iniziano ad accendersi")]
    [Range(0f, 24f)]
    public float neonOnHour = 19f;

    [Tooltip("Ora in cui i neon si spengono")]
    [Range(0f, 24f)]
    public float neonOffHour = 6.5f;

    [Tooltip("Velocità flicker dei neon (0 = nessun flicker)")]
    [Range(0f, 20f)]
    public float neonFlickerSpeed = 8f;

    [Tooltip("Ampiezza flicker (0 = stabile)")]
    [Range(0f, 1f)]
    public float neonFlickerAmount = 0.12f;

    // ─────────────────────────────────────────────
    // EVENTI
    // ─────────────────────────────────────────────
    [Header("── Callback Ora ────────────────────────")]
    [Tooltip("Invocato quando scatta l'alba (circa 6:00)")]
    public UnityEngine.Events.UnityEvent OnDawn;

    [Tooltip("Invocato quando scatta il tramonto (circa 19:00)")]
    public UnityEngine.Events.UnityEvent OnDusk;

    // ─────────────────────────────────────────────
    // PRIVATI
    // ─────────────────────────────────────────────
    private float _prevHour;
    private bool  _dawnFired;
    private bool  _duskFired;

    // ─────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────
    void OnEnable()
    {
        InitDefaultGradients();
        ApplyAll();
    }

    void Update()
    {
        if (timeRunning && Application.isPlaying)
        {
            float hoursPerSecond = 24f / dayDurationSeconds;
            currentHour = (currentHour + hoursPerSecond * Time.deltaTime) % 24f;
        }

        ApplyAll();
        FireEvents();
    }

    // ─────────────────────────────────────────────
    // CORE
    // ─────────────────────────────────────────────
    void ApplyAll()
    {
        float t = currentHour / 24f; // normalizzato 0-1

        UpdateSun(t);
        UpdateMoon(t);
        UpdateAmbient(t);
        UpdateFog(t);
        UpdateNeon();
    }

    /// <summary>Rotazione e colore del sole.</summary>
    void UpdateSun(float t)
    {
        if (sunLight == null) return;

        // Il sole sorge a Est (rotX=0 = sotto l'orizzonte, 180 = tramonto)
        // Mappa l'ora su -90° (mezzanotte) → 90° (mezzogiorno) → 270° (mezzanotte)
        float sunAngle = (t * 360f) - 90f; // -90° alle 0:00, +90° a mezzogiorno
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f);

        // Intensità: solo quando sopra l'orizzonte
        float sunElevation = Mathf.Sin(t * Mathf.PI * 2f - Mathf.PI * 0.5f);
        sunLight.intensity = Mathf.Clamp01(sunElevation) * sunMaxIntensity;

        // Colore dal gradiente VCS
        sunLight.color = sunColorGradient.Evaluate(t);
    }

    /// <summary>Luce lunare: opposta al sole, attiva solo di notte.</summary>
    void UpdateMoon(float t)
    {
        if (moonLight == null) return;

        float moonAngle = ((t + 0.5f) % 1f) * 360f - 90f;
        moonLight.transform.rotation = Quaternion.Euler(moonAngle, -30f, 0f);

        float moonElevation = Mathf.Sin((t + 0.5f) * Mathf.PI * 2f - Mathf.PI * 0.5f);
        float dayBlend = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI * 2f - Mathf.PI * 0.5f));
        moonLight.intensity = Mathf.Clamp01(moonElevation) * moonMaxIntensity * (1f - dayBlend);
    }

    /// <summary>Luce ambientale e colore cielo.</summary>
    void UpdateAmbient(float t)
    {
        // Sky color dal gradiente
        Color sky = skyColorGradient.Evaluate(t);
        RenderSettings.ambientSkyColor = sky;

        // Intensità ambientale
        float dayFactor = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI * 2f - Mathf.PI * 0.5f));
        RenderSettings.ambientIntensity = Mathf.Lerp(ambientNight, ambientDay, dayFactor);
    }

    /// <summary>Fog dinamico: più denso all'alba/tramonto e di notte.</summary>
    void UpdateFog(float t)
    {
        if (!dynamicFog) return;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;

        // Picco di fog: alba (~0.25 = 6:00) e tramonto (~0.79 = 19:00)
        float fogPeak = Mathf.Max(
            Mathf.Exp(-Mathf.Pow((t - 0.25f) * 8f, 2f)),   // alba
            Mathf.Exp(-Mathf.Pow((t - 0.79f) * 8f, 2f)),   // tramonto
            Mathf.Clamp01(1f - Mathf.Sin(t * Mathf.PI * 2f - Mathf.PI * 0.5f)) * 0.5f // notte
        );

        RenderSettings.fogDensity = fogPeak * fogDensityMax;
        RenderSettings.fogColor   = fogColorGradient.Evaluate(t);
    }

    /// <summary>Accende/spegne e flickera i neon notturni.</summary>
    void UpdateNeon()
    {
        if (neonLights == null || neonLights.Length == 0) return;

        bool isNight = (currentHour >= neonOnHour || currentHour < neonOffHour);

        // Fade in/out morbido intorno alle soglie
        float fadeIn  = Mathf.InverseLerp(neonOnHour - 0.5f,  neonOnHour,  currentHour);
        float fadeOut = Mathf.InverseLerp(neonOffHour + 0.5f, neonOffHour, currentHour);
        float neonT   = isNight ? Mathf.Max(fadeIn, fadeOut) : 0f;

        float flicker = 1f;
        if (neonFlickerSpeed > 0f && neonFlickerAmount > 0f)
        {
            flicker = 1f - neonFlickerAmount * (0.5f + 0.5f * Mathf.Sin(Time.time * neonFlickerSpeed
                       + Mathf.Sin(Time.time * neonFlickerSpeed * 0.37f)));
        }

        foreach (var neon in neonLights)
        {
            if (neon == null) continue;
            neon.enabled = neonT > 0.01f;
            if (neon.enabled)
            {
                // Conserva l'intensità originale salvata
                neon.intensity = neon.intensity * neonT * flicker;
            }
        }
    }

    // ─────────────────────────────────────────────
    // EVENTI ORA
    // ─────────────────────────────────────────────
    void FireEvents()
    {
        // Alba: 6:00
        if (!_dawnFired && currentHour >= 6f && currentHour < 7f)
        {
            _dawnFired = true;
            _duskFired = false;
            OnDawn?.Invoke();
        }
        // Tramonto: 19:00
        if (!_duskFired && currentHour >= 19f && currentHour < 20f)
        {
            _duskFired = true;
            _dawnFired = false;
            OnDusk?.Invoke();
        }
    }

    // ─────────────────────────────────────────────
    // GRADIENTS DI DEFAULT (stile VCS)
    // ─────────────────────────────────────────────
    void InitDefaultGradients()
    {
        // Colore Sole
        if (sunColorGradient == null || sunColorGradient.colorKeys.Length == 0)
        {
            sunColorGradient = new Gradient();
            sunColorGradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(0.20f, 0.10f, 0.30f), 0.00f), // notte
                    new GradientColorKey(new Color(1.00f, 0.45f, 0.10f), 0.25f), // alba arancio
                    new GradientColorKey(new Color(1.00f, 0.95f, 0.80f), 0.50f), // giorno
                    new GradientColorKey(new Color(1.00f, 0.40f, 0.60f), 0.79f), // tramonto magenta
                    new GradientColorKey(new Color(0.20f, 0.10f, 0.30f), 1.00f), // notte
                },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            );
        }

        // Colore Cielo
        if (skyColorGradient == null || skyColorGradient.colorKeys.Length == 0)
        {
            skyColorGradient = new Gradient();
            skyColorGradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(0.02f, 0.02f, 0.08f), 0.00f), // notte profonda
                    new GradientColorKey(new Color(0.60f, 0.25f, 0.50f), 0.22f), // pre-alba viola
                    new GradientColorKey(new Color(1.00f, 0.55f, 0.20f), 0.28f), // alba arancio
                    new GradientColorKey(new Color(0.40f, 0.70f, 1.00f), 0.50f), // giorno azzurro
                    new GradientColorKey(new Color(1.00f, 0.35f, 0.70f), 0.79f), // tramonto rosa/magenta
                    new GradientColorKey(new Color(0.15f, 0.05f, 0.25f), 0.88f), // imbrunire viola
                    new GradientColorKey(new Color(0.02f, 0.02f, 0.08f), 1.00f), // notte
                },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            );
        }

        // Colore Fog
        if (fogColorGradient == null || fogColorGradient.colorKeys.Length == 0)
        {
            fogColorGradient = new Gradient();
            fogColorGradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(0.04f, 0.04f, 0.12f), 0.00f), // notte
                    new GradientColorKey(new Color(0.80f, 0.40f, 0.50f), 0.25f), // alba
                    new GradientColorKey(new Color(0.80f, 0.88f, 1.00f), 0.50f), // giorno
                    new GradientColorKey(new Color(0.90f, 0.30f, 0.55f), 0.79f), // tramonto
                    new GradientColorKey(new Color(0.04f, 0.04f, 0.12f), 1.00f), // notte
                },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            );
        }
    }

#if UNITY_EDITOR
    // Preview in editor senza play
    void OnValidate() => ApplyAll();
#endif
}
