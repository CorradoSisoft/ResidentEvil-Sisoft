using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject gameplayPanel;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip introSound;

    [Header("Intro")]
    public IntroSequenceData introSequenceData;

    void Start()
    {
        Time.timeScale = 0f;
        mainMenuPanel.SetActive(true);
        gameplayPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        FadeManager.Instance.SetFadeInstant(0f);
        GameManager.Instance.SetState(GameState.MainMenu);
    }

    public void Play()
    {
        StartCoroutine(PlaySequence());
        /* mainMenuPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; */
    }

    private IEnumerator PlaySequence()
    {
        MenuCursor menuCursor = FindObjectOfType<MenuCursor>();
        if (menuCursor != null) menuCursor.enabled = false;

        // 1. Suono + lampo
        if (audioSource != null && introSound != null)
        {
            audioSource.PlayOneShot(introSound);
            yield return StartCoroutine(FadeManager.Instance.FlashRoutine(3, 0.24f)); // lampo immediato
            yield return new WaitForSecondsRealtime(introSound.length - 0.48f); // aspetta il resto del suono
        }

        // 2. Fadeout
        yield return FadeManager.Instance.FadeOutRoutine();

        // 3. Swap pannelli a schermo nero
        mainMenuPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameManager.Instance.SetState(GameState.Intro);

        // 4. Fadein
       /*  yield return FadeManager.Instance.FadeInRoutine(); */

        // 5. Avvia intro direttamente
        if (introSequenceData != null && IntroSequencePlayer.Instance != null)
            IntroSequencePlayer.Instance.Play(introSequenceData, () =>
                GameManager.Instance.SetState(GameState.Exploration));
        else
        {
            yield return FadeManager.Instance.FadeInRoutine(); // solo se non c'è intro
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}