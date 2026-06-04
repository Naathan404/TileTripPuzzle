using System;
using UnityEngine;

public enum GameState
{
    Playing,
    Win,
    Lose,
    Pause
}

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentGameState { get; private set; } = GameState.Playing;

    public static event Action OnLevelWin;
    public static event Action OnLevelLose;


    private void OnEnable()
    {
        BarManager.OnBarFull += HandleLoseLevel;
        Board.OnBoardClear += HandleWinLevel;
    }

    private void OnDisable()
    {
        BarManager.OnBarFull -= HandleLoseLevel;
        Board.OnBoardClear -= HandleWinLevel;
    }

    private void HandleWinLevel()
    {
        if (CurrentGameState != GameState.Playing) return;
        if(!BarManager.Instance.IsEmpty) return;

        Debug.Log("[GAME MANAGER] Win level");
        CurrentGameState = GameState.Win;
        OnLevelWin?.Invoke();
    }

    private void HandleLoseLevel()
    {
        if (CurrentGameState != GameState.Playing) return;

        Debug.Log("[GAME MANAGER] Lose level");
        CurrentGameState = GameState.Lose;
        OnLevelLose?.Invoke();
    }

    public void NextLevel()
    {
        CurrentGameState = GameState.Playing;
        Reset();

        Debug.Log("[GAME MANAGER] Next level");
    }

    public void ReplayLevel()
    {
        CurrentGameState = GameState.Playing;
        Reset();

        Debug.Log("[GAME MANAGER] Replay level");
    }

    public void Reset()
    {
        BarManager.Instance.ClearBar();
    }
}
