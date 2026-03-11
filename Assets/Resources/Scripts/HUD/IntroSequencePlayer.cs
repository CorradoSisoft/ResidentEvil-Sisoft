using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// ============================================================
//  INTRO SEQUENCE PLAYER
//  Player modulare per sequenze cinematografiche.
//  Usa ScriptableObject IntroSequenceData.
//  
//  USO:
//  - IntroSequencePlayer.Instance.Play(sequenceData);
//  - Riutilizzabile per prologo, capitoli, epilogo
// ============================================================

public class IntroSequencePlayer : MonoBehaviour
{
    public static IntroSequencePlayer Instance;

    [Header("── UI ──")]
    public GameObject introPanel;
    public TextMeshProUGUI introText;
    public Image introImage;

    [Header("── Image Pan Effect ──")]
    public float panAmount = 20f;
    public float panSpeed = 0.5f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (introPanel != null)
            introPanel.SetActive(false);
    }

    /// <summary>Play una sequenza da ScriptableObject</summary>
    public void Play(IntroSequenceData sequenceData, System.Action onComplete = null)
    {
        if (sequenceData == null)
        {
            Debug.LogError("[IntroPlayer] sequenceData è NULL!");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(PlaySequence(sequenceData, onComplete));
    }

    private IEnumerator PlaySequence(IntroSequenceData data, System.Action onComplete)
    {
        Debug.Log($"[IntroPlayer] Inizio sequenza: {data.sequenceName}");
        
        if (FadeManager.Instance != null)
            FadeManager.Instance.SetFadeInstant(1f);

        if (introPanel != null)
            introPanel.SetActive(true);

        foreach (var line in data.lines)
        {
            SetupImage(line.image);
            introText.text = "";
            introText.fontSize = line.textSize;

            // Fade in immagine
            yield return FadeImage(0f, 1f, line.fadeSpeed);

            // Typewriter testo
            yield return TypewriterText(line.text, line.typewriterSpeed);

            // Aspetta lineDelay skippabile
            float elapsed = 0f;
            while (elapsed < line.lineDelay)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    break;
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Fade out tutto
            yield return FadeVisuals(1f, 0f, line.fadeSpeed);
            yield return new WaitForSeconds(0.5f);
        }

        if (introPanel != null)
            introPanel.SetActive(false);

        if (FadeManager.Instance != null)
            yield return FadeManager.Instance.FadeInRoutine();

        if (!string.IsNullOrEmpty(data.completionFlag))
        {
            GameManager.Instance.SetFlag(data.completionFlag);
            Debug.Log($"[IntroPlayer] Flag settato: {data.completionFlag}");
        }

        GameManager.Instance.SetState(GameState.Exploration);
        Debug.Log($"[IntroPlayer] Sequenza completata: {data.sequenceName}");

        onComplete?.Invoke();
    }

    
    private void SetupImage(Sprite sprite)
    {
        if (introImage == null)
            return;

        RectTransform rect = introImage.rectTransform;
        rect.anchoredPosition = Vector2.zero;

        if (sprite != null)
        {
            introImage.sprite = sprite;
            introImage.color = Color.white;
        }
        else
        {
            // Nessuna immagine → nero pieno
            introImage.sprite = null;
            introImage.color = Color.black;
        }
    }

    private IEnumerator FadeVisuals(float startAlpha, float endAlpha, float speed)
    {
        float elapsed = 0f;

        Color textColor = introText.color;
        Color imageColor = introImage != null ? introImage.color : Color.black;

        RectTransform imageRect = introImage != null ? introImage.rectTransform : null;
        Vector2 startPos = imageRect != null ? imageRect.anchoredPosition : Vector2.zero;

        while (elapsed < speed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / speed;

            // Fade testo
            textColor.a = Mathf.Lerp(startAlpha, endAlpha, t);
            introText.color = textColor;

            // Fade immagine
            if (introImage != null)
            {
                imageColor.a = Mathf.Lerp(startAlpha, endAlpha, t);
                introImage.color = imageColor;

                // Pan solo se c'è sprite
                if (imageRect != null && introImage.sprite != null)
                {
                    float panOffset = Mathf.Sin(Time.time * panSpeed) * panAmount;
                    imageRect.anchoredPosition = startPos + new Vector2(panOffset, 0);
                }
            }

            yield return null;
        }

        // Assicura valori finali esatti
        textColor.a = endAlpha;
        introText.color = textColor;

        if (introImage != null)
        {
            imageColor.a = endAlpha;
            introImage.color = imageColor;
        }
    }

    private IEnumerator TypewriterText(string fullText, float typewriterSpeed)
    {
        introText.text = "";
        introText.color = new Color(introText.color.r, introText.color.g, introText.color.b, 1f);

        foreach (char c in fullText)
        {
            introText.text += c;

            // Skip immediato con Invio — mostra tutto il testo
            if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
            {
                introText.text = fullText;
                yield break;
            }

            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    private IEnumerator FadeImage(float startAlpha, float endAlpha, float speed)
    {
        if (introImage == null) yield break;

        float elapsed = 0f;
        Color imageColor = introImage.color;

        while (elapsed < speed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / speed;
            imageColor.a = Mathf.Lerp(startAlpha, endAlpha, t);
            introImage.color = imageColor;
            yield return null;
        }

        imageColor.a = endAlpha;
        introImage.color = imageColor;
    }
}
