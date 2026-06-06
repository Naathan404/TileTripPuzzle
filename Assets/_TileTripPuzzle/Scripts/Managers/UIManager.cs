using UnityEngine;
using UnityEngine.Rendering;

public class UIManager : Singleton<UIManager>
{
    [Header("UI Components")]
    [SerializeField] private GameObject _playingPanel;
    [SerializeField] private GameObject _winLevelPanel;
    [SerializeField] private GameObject _loseLevelPanel;
    [SerializeField] private GameObject _pausePanel;

    private void OnEnable()
    {
        GameManager.OnLevelWin += ShowWinPanel;
        GameManager.OnLevelLose += ShowLosePanel;
    }

    private void OnDisable()
    {
        GameManager.OnLevelWin -= ShowWinPanel;
        GameManager.OnLevelLose -= ShowLosePanel;
    }

    private void Start()
    {
        _winLevelPanel.SetActive(false);
        _loseLevelPanel.SetActive(false);
        _pausePanel.SetActive(false);
    }

    private void ShowWinPanel()
    {
        _winLevelPanel.SetActive(true);
        _playingPanel.SetActive(false);
    }


    private void ShowLosePanel()
    {
        _loseLevelPanel.SetActive(true);
        _playingPanel.SetActive(false);
    }

    public void OnPauseButtonClicked()
    {
        if(GameManager.Instance.CurrentGameState != GameState.Playing) return;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Button);
        _pausePanel.SetActive(true);
        _playingPanel.SetActive(false);
        GameManager.Instance.SetGameState(GameState.Pause);
    }

    public void OnNextLevelButtonClicked()
    {
        DebugManager.Instance.Log("[UI MANAGER] Next Level Button click");
        GameManager.Instance.NextLevel();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Button);
        _winLevelPanel.SetActive(false);
        _playingPanel.SetActive(true);
    }

    public void OnReplayButtonClicked()
    {
        DebugManager.Instance.Log("[UI MANAGER] Replay Button click");
        GameManager.Instance.ReplayLevel();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Button);
        _winLevelPanel.SetActive(false);
        _loseLevelPanel.SetActive(false);
        _playingPanel.SetActive(true);
    }

    public void OnContinueButtonClicked()
    {
        DebugManager.Instance.Log("[UI MANAGER] Continue Button click");
        GameManager.Instance.SetGameState(GameState.Playing);
        _pausePanel.SetActive(false);
        _playingPanel.SetActive(true);
    }
}
