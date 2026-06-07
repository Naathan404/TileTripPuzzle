using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject _playingPanel;
    [SerializeField] private GameObject _winLevelPanel;
    [SerializeField] private GameObject _loseLevelPanel;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _settingPanel;

    [Header("Audio Sliders")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI _levelNumberText;

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
        _playingPanel.SetActive(true);
        _winLevelPanel.SetActive(false);
        _loseLevelPanel.SetActive(false);
        _pausePanel.SetActive(false);
        _settingPanel.SetActive(false);

        UpdateSlider();
    }

    private void UpdateSlider()
    {
        float musicVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        if (_musicSlider != null) _musicSlider.value = musicVolume;
        if (_sfxSlider != null) _sfxSlider.value = sfxVolume;
    }

    private void ShowWinPanel()
    {
        _playingPanel.SetActive(false);
        _winLevelPanel.SetActive(true);
        _loseLevelPanel.SetActive(false);
        _pausePanel.SetActive(false);
        _settingPanel.SetActive(false);
    }


    private void ShowLosePanel()
    {
        _playingPanel.SetActive(false);
        _winLevelPanel.SetActive(false);
        _loseLevelPanel.SetActive(true);
        _pausePanel.SetActive(false);
        _settingPanel.SetActive(false);
    }

    public void OnPauseButtonClicked()
    {
        if(GameManager.Instance.CurrentGameState != GameState.Playing) return;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Button);
        GameManager.Instance.SetGameState(GameState.Pause);
        _playingPanel.SetActive(false);
        _winLevelPanel.SetActive(false);
        _loseLevelPanel.SetActive(false);
        _pausePanel.SetActive(true);
        _settingPanel.SetActive(false);
    }

    public void OnNextLevelButtonClicked()
    {
        DebugManager.Instance.Log("[UI MANAGER] Next Level Button click");
        GameManager.Instance.NextLevel();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Button);
        _playingPanel.SetActive(true);
        _winLevelPanel.SetActive(false);
        _loseLevelPanel.SetActive(false);
        _pausePanel.SetActive(false);
        _settingPanel.SetActive(false);
    }

    public void OnReplayButtonClicked()
    {
        DebugManager.Instance.Log("[UI MANAGER] Replay Button click");
        GameManager.Instance.ReplayLevel();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Button);
        _playingPanel.SetActive(true);
        _winLevelPanel.SetActive(false);
        _loseLevelPanel.SetActive(false);
        _pausePanel.SetActive(false);
        _settingPanel.SetActive(false);
    }

    public void OnContinueButtonClicked()
    {
        DebugManager.Instance.Log("[UI MANAGER] Continue Button click");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Button);
        GameManager.Instance.SetGameState(GameState.Playing);
        _playingPanel.SetActive(true);
        _winLevelPanel.SetActive(false);
        _loseLevelPanel.SetActive(false);
        _pausePanel.SetActive(false);
        _settingPanel.SetActive(false);
    }

    public void OnSettingButtonClicked()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Button);
        _playingPanel.SetActive(false);
        _winLevelPanel.SetActive(false);
        _loseLevelPanel.SetActive(false);
        _pausePanel.SetActive(false);
        _settingPanel.SetActive(true);
    }

    public void BackToPauseButtonClicked()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Button);
        _playingPanel.SetActive(false);
        _winLevelPanel.SetActive(false);
        _loseLevelPanel.SetActive(false);
        _pausePanel.SetActive(true);
        _settingPanel.SetActive(false);
    }


    public void OnMusicSliderChanged(float value)
    {
        DebugManager.Instance.Log("[UI MANAGER] Thay đổi âm lượng cho BGM");
        PlayerPrefs.SetFloat("BGMVolume", value);
        AudioManager.Instance.SetVolume();
    }

    public void OnSfxSliderChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        
        AudioManager.Instance.SetVolume();
    }

    public void UpdateLevelUI(int levelNumber)
    {
        _levelNumberText.text = $"LEVEL {levelNumber}";
    }
}
