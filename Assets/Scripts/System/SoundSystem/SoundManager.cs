using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Volume")]
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    
    [Header("AudioMixer")]
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    // ---------- Lifecycle ----------
    public void InitByGameManager()
    {
        InitSingleton();
        CreateAudioSourcesIfNeeded();
        DontDestroyOnLoad(gameObject);
    }

    // ---------- Public: BGM ----------
    public void PlayBGM(BgmEntry bgmEntry)
    {
        if (bgmEntry == null)
        {
            Debug.LogError("BGM Entry is null");
            return;
        }
        
        PrepareBGM(bgmEntry);
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
    }

    // ---------- Public: SFX ----------
    public void PlaySFX(AudioClip clip, float pitch = 1f)
    {
        if (clip == null) return;
        
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, sfxVolume);
        sfxSource.pitch = 1f;
    }
    
    // ---------- Audio Mixer ----------
    public void SetMasterVolume(float value)  // value: 0~1
    {
        float db = Mathf.Lerp(-80f, 0f, value);
        audioMixer.SetFloat("MasterVolume", db);
    }

    public void SetBGMVolume(float value)
    {
        float db = Mathf.Lerp(-80f, 0f, value);
        audioMixer.SetFloat("BGMVolume", db);
    }

    public void SetSFXVolume(float value)
    {
        float db = Mathf.Lerp(-80f, 0f, value);
        audioMixer.SetFloat("SFXVolume", db);
    }

    // ---------- Helpers ----------
    private void InitSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void CreateAudioSourcesIfNeeded()
    {
        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        ConfigureSources();
    }

    private void ConfigureSources()
    {
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    private void PrepareBGM(BgmEntry bgmEntry)
    {
        bgmSource.clip = bgmEntry.clip;
        bgmSource.loop = bgmEntry.loop;
        bgmSource.volume = bgmEntry.bgmVolume;
    }

    // ---------- Optional: 실시간 볼륨 반영 ----------
    private void OnValidate()
    {
        if (bgmSource != null) bgmSource.volume = bgmVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }
}
