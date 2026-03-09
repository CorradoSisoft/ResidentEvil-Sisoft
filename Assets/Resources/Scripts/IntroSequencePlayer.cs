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
        
        // Schermo nero iniziale
        if (FadeManager.Instance != null)
            FadeManager.Instance.SetFadeInstant(1f);

        if (introPanel != null)
            introPanel.SetActive(true);

        foreach (var line in data.lines)
        {
            SetupImage(line.image);

            introText.text = line.text;
            introText.fontSize = line.textSize;

            // Fade in
            yield return FadeVisuals(0f, 1f, line.fadeSpeed);

            // Mostra testo
            yield return new WaitForSeconds(line.lineDelay);

            // Fade out
            yield return FadeVisuals(1f, 0f, line.fadeSpeed);

            // Pausa tra righe
            yield return new WaitForSeconds(0.5f);
        }

        if (introPanel != null)
            introPanel.SetActive(false);

        // Fade in gameplay
        if (FadeManager.Instance != null)
            yield return FadeManager.Instance.FadeInRoutine();

        // ← POST-SEQUENCE ACTIONS
        
        // Setta flag se specificato
        if (!string.IsNullOrEmpty(data.completionFlag))
        {
            GameManager.Instance.SetFlag(data.completionFlag);
            Debug.Log($"[IntroPlayer] Flag settato: {data.completionFlag}");
        }

        GameManager.Instance.SetState(GameState.Exploration);

        Debug.Log($"[IntroPlayer] Sequenza completata: {data.sequenceName}");

        // ← Per triggerare cutscene dopo intro, usa EventTrigger con flagCondition
        //Esempio: EventTrigger con flagCondition="prologo_completed"
        //avviare la cutscene
        /* CutsceneTrigger cutsceneTrigger = FindObjectOfType<CutsceneTrigger>();
        Debug.Log($"[IntroPlayer] CutsceneTrigger trovato: {(cutsceneTrigger != null ? cutsceneTrigger.gameObject.name : "Nessuno")}");
        if (cutsceneTrigger != null) cutsceneTrigger.OnTriggerEnter2D(FindObjectOfType<PlayerController>().GetComponent<Collider2D>()); */

        
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
}
