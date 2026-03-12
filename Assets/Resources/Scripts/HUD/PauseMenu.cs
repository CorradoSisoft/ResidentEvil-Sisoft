using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused = false;

    public GameObject pauseMenuUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!IsPaused && InventoryManager.Instance.IsOpen) return;
            if (!IsPaused && SafeLock.IsAnyOpen) return;
            if (!IsPaused && SafeLock._justClosed) { SafeLock._justClosed = false; return; } // ← qui
            if (IsPaused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //BackgroundMusic.Instance.Stop(); // ← ferma musica
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //BackgroundMusic.Instance.PlayFloor(); // ← riprende musica
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

        SaveManager.Instance.Load();

        yield return StartCoroutine(FadeManager.Instance.FadeInRoutine());
    }

    private IEnumerator BackToMenuSequence()
    {
        IsPaused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;

        yield return StartCoroutine(FadeManager.Instance.FadeOutRoutine());

        MainMenu mainMenu = FindObjectOfType<MainMenu>(true);
        mainMenu.gameplayPanel.SetActive(false);
        mainMenu.mainMenuPanel.SetActive(true);
        BackgroundMusic.Instance.Stop();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameManager.Instance.SetState(GameState.MainMenu);

        MenuCursor menuCursor = FindObjectOfType<MenuCursor>();
        if (menuCursor != null)
        {
            menuCursor.enabled = true;
            menuCursor.menuItemsCount = SaveManager.Instance.SaveExists() ? 3 : 2;
            menuCursor.RebuildLayout();
        }

        yield return StartCoroutine(FadeManager.Instance.FadeInRoutine());
    }
}