using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused = false;

    public GameObject pauseMenuUI;

    [Header("Pulsanti")]
    public GameObject loadButton;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!IsPaused && InventoryManager.Instance.IsOpen) return;
            if (!IsPaused && SafeLock.IsAnyOpen) return;
            if (!IsPaused && SafeLock._justClosed) { SafeLock._justClosed = false; return; }
            if (IsPaused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        if (loadButton != null)
            loadButton.SetActive(SaveManager.Instance.SaveExists());

        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ← aggiunto
        MenuCursor menuCursor = pauseMenuUI.GetComponentInChildren<MenuCursor>();
        if (menuCursor != null)
            menuCursor.RebuildLayout();
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        IsPaused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;

        yield return StartCoroutine(FadeManager.Instance.FadeOutRoutine());

        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) pm.OnPlayerRespawn();

        yield return null;

        SaveManager.Instance.Load();

        yield return StartCoroutine(FadeManager.Instance.FadeInRoutine());
    }

    private IEnumerator BackToMenuSequence()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        yield return StartCoroutine(FadeManager.Instance.FadeOutRoutine());

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}