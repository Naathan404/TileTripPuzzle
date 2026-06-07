using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    public static AudioManager instance;
    [SerializeField] private float fadeOutDuration;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] AudioSource BGMSource;
    [SerializeField] AudioSource SFXSource;

    [Header("BGM")]
    public AudioClip BGM_1;

    [Header("SFX")]
    public AudioClip SFX_Match;
    public AudioClip SFX_Tap;
    public AudioClip SFX_Victory;
    public AudioClip SFX_Victory_1;
    public AudioClip SFX_Fail;
    public AudioClip SFX_Blocked;
    public AudioClip SFX_Button;

    private void Start()
    {
        BGMSource.loop = true;
        SetVolume();

        PlayMusic(BGM_1);
    }

    public void StopAll()
    {
        BGMSource.Stop();
        SFXSource.Stop();
    }

    public void StopBGM()
    {
        BGMSource.Stop();
    }

    public void PlayMusic(AudioClip bgm, bool onlyPlayThis = false)
    {
        if(onlyPlayThis)
            BGMSource.Stop();
        BGMSource.clip = bgm;
        BGMSource.Play();
    }

    public void PlaySFX(AudioClip sfx, float volume = 1f)
    {
        if(sfx == null) return;
        SFXSource.PlayOneShot(sfx, volume);
    }

    public void FadeOutMusic()
    {
        StartCoroutine(FadeOutMusicCoroutine());
    }

    private IEnumerator FadeOutMusicCoroutine()
    {
        float startVolume = BGMSource.volume;

        float time = 0f;
        while (time < fadeOutDuration)
        {
            time += Time.unscaledDeltaTime;
            BGMSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeOutDuration);
            yield return null;
        }

        BGMSource.Stop();
        BGMSource.volume = startVolume;
    }

    public void SetVolume()
    {
        float musicVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(musicVolume) * 20);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
    }

    public void PauseMusic()
    {
        if(BGMSource.isPlaying)
            BGMSource.Pause();
    }

    public void UnPauseMusic()
    {   
        if(!BGMSource.isPlaying)
            BGMSource.UnPause();
    }
}
