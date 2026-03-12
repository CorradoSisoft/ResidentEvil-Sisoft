using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EndGameSequence : MonoBehaviour
{
    public static EndGameSequence Instance;

    [Header("UI")]
    public GameObject endGamePanel;
    public TextMeshProUGUI creditsText;

    [Header("Impostazioni")]
    public float fadeOutDuration = 2f;
    public float textScrollSpeed = 50f;    // pixel al secondo
    public float pauseBeforeScroll = 2f;   // attesa prima che parta il testo
    public float pauseAfterScroll = 3f;    // attesa dopo la fine del testo

    private RectTransform creditsRect;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartEnding()
    {
        StartCoroutine(EndingSequence());
    }

    private IEnumerator EndingSequence()
    {
        // 1. Blocca tutto
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. Fade a nero
        yield return StartCoroutine(FadeManager.Instance.FadeOutRoutine(fadeOutDuration));

        // 3. Attiva il panel credits
        endGamePanel.SetActive(true);
        creditsRect = creditsText.GetComponent<RectTransform>();

        // Posiziona il testo sotto lo schermo
        float screenHeight = Screen.height;
        creditsRect.anchoredPosition = new Vector2(0, -screenHeight);

        // 4. Fade in sul panel credits
        yield return StartCoroutine(FadeManager.Instance.FadeInRoutine(1.5f));

        // 5. Pausa prima dello scroll
        yield return new WaitForSecondsRealtime(pauseBeforeScroll);

        // 6. Scrolla il testo dal basso verso l'alto
        float textHeight = creditsRect.rect.height;
        float totalDistance = screenHeight + textHeight;
        float duration = totalDistance / textScrollSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float yPos = Mathf.Lerp(-screenHeight, screenHeight + textHeight, t);
            creditsRect.anchoredPosition = new Vector2(0, yPos);
            yield return null;
        }

        // 7. Pausa finale
        yield return new WaitForSecondsRealtime(pauseAfterScroll);

        // 8. Fade a nero
        yield return StartCoroutine(FadeManager.Instance.FadeOutRoutine(1.5f));

        // 9. Torna al menu
        ReturnToMenu();
    }

    private void ReturnToMenu()
    {
        endGamePanel.SetActive(false);

        // Reset salvataggio
        /* SaveManager.Instance.DeleteSave(); */

        // Torna al menu esattamente come fa PauseMenu.BackToMenu
        MainMenu mainMenu = FindObjectOfType<MainMenu>(true);
        mainMenu.gameplayPanel.SetActive(false);
        mainMenu.mainMenuPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameManager.Instance.SetState(GameState.MainMenu);
        BackgroundMusic.Instance.Stop();

        MenuCursor menuCursor = FindObjectOfType<MenuCursor>();
        if (menuCursor != null)
        {
            menuCursor.enabled = true;
            menuCursor.menuItemsCount = SaveManager.Instance.SaveExists() ? 3 : 2;
            menuCursor.RebuildLayout();
        }

        StartCoroutine(FadeManager.Instance.FadeInRoutine());
    }
}