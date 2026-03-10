using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [Header("Flickering")]
    public float minIntensity = 0f;
    public float maxIntensity = 3f;

    [Header("Timing")]
    public float minFlickerSpeed = 0.02f;
    public float maxFlickerSpeed = 0.15f;
    public float minStableTime = 0.5f;
    public float maxStableTime = 3f;

    [Header("Buzz Sound (opzionale)")]
    public AudioSource buzzSound;

    private Light lt;
    private float originalIntensity;
    private float timer = 0f;
    private float nextFlickerTime = 0f;
    private bool isStable = false;
    private float stableTimer = 0f;

    void Start()
    {
        lt = GetComponent<Light>();
        originalIntensity = lt.intensity;
        ScheduleNextFlicker();
    }

    void Update()
    {
        if (isStable)
        {
            // Fase stabile: luce accesa normalmente
            lt.intensity = originalIntensity;
            stableTimer -= Time.deltaTime;
            if (stableTimer <= 0f)
            {
                isStable = false;
                ScheduleNextFlicker();
            }
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            // Flickering rapido casuale
            lt.intensity = Random.value > 0.3f
                ? Random.Range(minIntensity, maxIntensity)
                : 0f; // ogni tanto si spegne del tutto

            if (buzzSound != null && !buzzSound.isPlaying)
                buzzSound.Play();

            timer = Random.Range(minFlickerSpeed, maxFlickerSpeed);
            nextFlickerTime -= Time.deltaTime;

            if (nextFlickerTime <= 0f)
            {
                // Dopo un po' di flickering, torna stabile
                isStable = true;
                stableTimer = Random.Range(minStableTime, maxStableTime);
                lt.intensity = originalIntensity;

                if (buzzSound != null && buzzSound.isPlaying)
                    buzzSound.Stop();
            }
        }
    }

    void ScheduleNextFlicker()
    {
        // Quanto dura la fase di flickering
        nextFlickerTime = Random.Range(0.3f, 1.5f);
        timer = 0f;
    }
}