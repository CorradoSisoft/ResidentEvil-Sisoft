using UnityEngine;
using System.Collections.Generic;

public enum GameState { MainMenu, Intro, Exploration, Combat, Paused }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState currentState { get; private set; }
    private Dictionary<string, bool> flags = new Dictionary<string, bool>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"[GameManager] Stato: {newState}");
    }

    public void SetFlag(string flag)
    {
        flags[flag] = true;
    }

    public bool GetFlag(string flag)
    {
        return flags.ContainsKey(flag) && flags[flag];
    }
}