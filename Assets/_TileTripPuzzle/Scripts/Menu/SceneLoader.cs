using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private CanvasGroup _fadePanel;
    [SerializeField] private float _fadeDuration = 0.5f;

    public void LoadScene(string sceneName)
    {
        _fadePanel.alpha = 0f;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        _fadePanel.blocksRaycasts = true;
        yield return _fadePanel.DOFade(1f, _fadeDuration).WaitForCompletion();

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
            yield return null;

        operation.allowSceneActivation = true;

        yield return null;
        yield return _fadePanel.DOFade(0f, _fadeDuration).WaitForCompletion();
        _fadePanel.blocksRaycasts = false;
    }

    private void Start()
    {
        _fadePanel.alpha = 1f;
        _fadePanel.DOFade(0f, _fadeDuration);
        _fadePanel.blocksRaycasts = false;
    }
}