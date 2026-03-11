using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("UI")]
    public GameObject gameOverPanel;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip gameOverMusic;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        // Blocca tutto
        Time.timeScale = 0f;

        // Fade nero
        if (FadeManager.Instance != null)
            yield return StartCoroutine(FadeManager.Instance.FadeOutRoutine());

        // Mostra pannello
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Musica game over
        if (audioSource != null && gameOverMusic != null)
        {
            audioSource.clip = gameOverMusic;
            audioSource.loop = false;
            audioSource.Play();
        }

        // Cursore visibile
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadGame()
    {
        StartCoroutine(LoadGameSequence());
    }

    public void BackToMenu()
    {
        StartCoroutine(BackToMenuSequence());
    }

    private IEnumerator LoadGameSequence()
    {
        // Fade in per togliere il nero
        yield return StartCoroutine(FadeManager.Instance.FadeInRoutine());

        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);

        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) pm.OnPlayerRespawn();

        SaveManager.Instance.Load();
    }

    private IEnumerator BackToMenuSequence()
    {
        yield return StartCoroutine(FadeManager.Instance.FadeInRoutine());

        Time.timeScale = 0f;
        gameOverPanel.SetActive(false);

        MainMenu mainMenu = FindObjectOfType<MainMenu>(true); // true = cerca anche inattivi
        mainMenu.gameplayPanel.SetActive(false);
        mainMenu.mainMenuPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameManager.Instance.SetState(GameState.MainMenu);

        // Ricostruisce il menu cursor
        MenuCursor menuCursor = FindObjectOfType<MenuCursor>();
        if (menuCursor != null)
        {
            menuCursor.enabled = true;
            menuCursor.menuItemsCount = SaveManager.Instance.SaveExists() ? 3 : 2;
            menuCursor.RebuildLayout();
        }
    }
}