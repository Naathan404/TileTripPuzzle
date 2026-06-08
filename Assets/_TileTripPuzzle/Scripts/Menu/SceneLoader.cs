using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private CanvasGroup _fadePanel;
    [SerializeField] private float _fadeDuration = 1.0f;

    public void LoadScene(string sceneName)
    {
        _fadePanel.blocksRaycasts = true;
        _fadePanel.DOFade(1f, _fadeDuration)
            .OnComplete(() => 
            {
                AudioManager.Instance.StopBGM();
                SceneManager.LoadScene(sceneName);
            }); 
    }


    private void Start()
    {
        _fadePanel.gameObject.SetActive(true);
        _fadePanel.alpha = 1f;
        _fadePanel.DOFade(0f, _fadeDuration)
            .OnComplete(() => 
            {
                _fadePanel.blocksRaycasts = false;                
                AudioManager.Instance.PlayMusic(AudioManager.Instance.BGM_1);
            });
    }
}