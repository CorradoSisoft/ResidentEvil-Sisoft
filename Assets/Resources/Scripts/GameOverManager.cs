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
        Time.timeScale = 0f;

        if (FadeManager.Instance != null)
            yield return StartCoroutine(FadeManager.Instance.FadeOutRoutine());

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Transform loadButton = gameOverPanel.transform.Find("gameover_load_btn");
        if (loadButton != null)
            loadButton.gameObject.SetActive(SaveManager.Instance.SaveExists());

        // ← aggiunto
        MenuCursor menuCursor = gameOverPanel.GetComponentInChildren<MenuCursor>();
        if (menuCursor != null)
            menuCursor.RebuildLayout();

        if (audioSource != null && gameOverMusic != null)
        {
            audioSource.clip = gameOverMusic;
            audioSource.loop = false;
            audioSource.Play();
        }

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
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);

        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) pm.OnPlayerRespawn();

        yield return null;

        SaveManager.Instance.Load();

        yield return null;

        yield return StartCoroutine(FadeManager.Instance.FadeInRoutine());
    }

    private IEnumerator BackToMenuSequence()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);

        yield return StartCoroutine(FadeManager.Instance.FadeInRoutine());

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}