using System;
using UnityEngine;
using UnityEngine.Rendering;

public enum GameState
{
    Playing,
    Win,
    Lose,
    Pause
}

public class GameManager : MonoBehaviour
{
    public GameState CurrentGameState { get; private set; } = GameState.Playing;

    public static event Action OnLevelWin;
    public static event Action OnLevelLose;

    [Header("Player Stats")]
    public SaveData Data;
    public int HintUseCount = 10;
    [SerializeField] private int _totalLevels = 10;

    [Header("References")]
    [SerializeField] private LevelManager _levelManager;
    [SerializeField] private Board _board;

    #region Object Life cycle
    public static GameManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        Data = SaveSystem.Load();    
        HintUseCount = Data.HintUseCount;
    }

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

    #endregion

    /// <summary>
    /// Xử lý thắng level
    /// </summary>
    private void HandleWinLevel()
    {
        Debug.Log($"[GAME MANAGER] bar tile count: {BarManager.Instance.TileCount}");
        if (CurrentGameState != GameState.Playing) return;
        if(!BarManager.Instance.IsEmpty) return;

        AudioManager.Instance.PauseMusic();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Victory_1, 0.8f);
        Debug.Log("[GAME MANAGER] Win level");
        CurrentGameState = GameState.Win;
        OnLevelWin?.Invoke();
    }

    /// <summary>
    /// Xử lý thua level
    /// </summary>
    private void HandleLoseLevel()
    {
        if (CurrentGameState != GameState.Playing) return;

        AudioManager.Instance.PauseMusic();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Fail);
        Debug.Log("[GAME MANAGER] Lose level");
        CurrentGameState = GameState.Lose;
        HintUseCount = Data.HintUseCount;
        OnLevelLose?.Invoke();
    }

    /// <summary>
    /// Gọi để load màn tiếp theo
    /// </summary>
    public void NextLevel()
    {
        Debug.Log("[GAME MANAGER] Next level");
        CurrentGameState = GameState.Playing;
        Reset();

        int nextLevelId = Data.CurrentLevel + 1;
        if(nextLevelId > _totalLevels)
        {
            Debug.Log("[LEVEL MANAGER] Đã hoàn thành tất cả màn chơi");
            return;
        }
        Data.CurrentLevel = nextLevelId;
        Data.HintUseCount = HintUseCount;
        SaveSystem.Save(Data);
        AudioManager.Instance.UnPauseMusic();
        _levelManager.LoadNextLevel();
    }

    public void ReplayLevel()
    {
        Debug.Log("[GAME MANAGER] Replay level");
        CurrentGameState = GameState.Playing;
        Reset();
        AudioManager.Instance.PlayMusic(AudioManager.Instance.BGM_1, true);
        HintUseCount = Data.HintUseCount;
        _levelManager.ReplayLevel();

    }

    public void HandleUseHint()
    {
        if(HintUseCount <= 0)
        {
            DebugManager.Instance.LogWarning("[GAME MANAGER] Số lượt dùng hint đã hết");
            return;
        }

        _board.UseHintPowerUp();
        HintUseCount--;
    }

    #region Helpers
    /// <summary>
    /// Set trạng thái level lại từ đầu
    /// </summary>
    public void Reset()
    {
        Debug.Log("[GAME MANAGER] Reset");
    }

    public void SetGameState(GameState newState)
    {
        CurrentGameState = newState;
    }
    #endregion
}
