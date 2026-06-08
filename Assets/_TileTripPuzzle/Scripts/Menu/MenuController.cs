using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _settingPanel;

    [Header("Audio Sliders")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    private void Start()
    {
        UpdateSlider();
    }

    public void OnExitButtonClicked()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Button);
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void OnSettingButtonClicked()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Button);
        _menuPanel.SetActive(false);
        _settingPanel.SetActive(true);
    }

    public void OnMenuPanelButtonClicked()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Button);
        _menuPanel.SetActive(true);
        _settingPanel.SetActive(false);
    }

    private void UpdateSlider()
    {
        float musicVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        if (_musicSlider != null) _musicSlider.value = musicVolume;
        if (_sfxSlider != null) _sfxSlider.value = sfxVolume;
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
}
