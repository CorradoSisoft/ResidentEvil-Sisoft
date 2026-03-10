using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    public Image fadeImage;
    public float defaultDuration = 1f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetFadeInstant(float alpha)
    {
        fadeImage.color = new Color(0, 0, 0, alpha);
    }

    public void FadeIn(float duration = -1f)
    {
        StartCoroutine(DoFade(1f, 0f, duration < 0 ? defaultDuration : duration));
    }

    public void FadeOut(float duration = -1f)
    {
        StartCoroutine(DoFade(0f, 1f, duration < 0 ? defaultDuration : duration));
    }

    public IEnumerator FadeInRoutine(float duration = -1f)
    {
        yield return DoFade(1f, 0f, duration < 0 ? defaultDuration : duration);
    }

    public IEnumerator FadeOutRoutine(float duration = -1f)
    {
        yield return DoFade(0f, 1f, duration < 0 ? defaultDuration : duration);
    }

    private IEnumerator DoFade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // ← deve essere unscaled perché timeScale è 0
            float t = elapsed / duration;
            fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(from, to, t));
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, to);
    }

    public IEnumerator FlashRoutine(int flashes = 3, float flashSpeed = 0.08f)
    {
        for (int i = 0; i < flashes; i++)
        {
            fadeImage.color = new Color(1, 1, 1, 1); // bianco
            yield return new WaitForSecondsRealtime(flashSpeed);
            fadeImage.color = new Color(1, 1, 1, 0); // trasparente
            yield return new WaitForSecondsRealtime(flashSpeed);
        }
    }
}