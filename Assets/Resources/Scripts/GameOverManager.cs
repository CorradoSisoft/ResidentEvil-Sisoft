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
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);

        // Ripristina player
        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) pm.OnPlayerRespawn();

        SaveManager.Instance.Load();
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // ← nome della tua scena menu
    }
}