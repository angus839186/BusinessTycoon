using System;
using UnityEngine;
using UnityEngine.Audio;

public sealed class AudioSystem : MonoBehaviour
{
    private const string MusicVolumeParameter = "MusicVolume";
    private const string SfxVolumeParameter = "SfxVolume";
    private const float MinimumDecibels = -80f;

    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;
    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Library")]
    [SerializeField]
    private SfxLibrary sfxLibrary;

    public static AudioSystem Instance
    {
        get;
        private set;
    }

    public SoundSettings Settings
    {
        get;
        private set;
    }

    public AudioSource MusicSource => musicSource;
    public event Action SettingsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        Settings = SoundSettingsStore.Load();

        ApplyVolumes();

        if (mixer == null)
        {
            Debug.LogError("AudioSystem 尚未設定 AudioMixer。", this);
        }

        if (musicSource == null)
        {
            Debug.LogError("AudioSystem 尚未設定 Music AudioSource。", this);
        }

        if (sfxSource == null)
        {
            Debug.LogError("AudioSystem 尚未設定 SFX AudioSource。", this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetMusicVolume(float volume)
    {
        Settings.musicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolume();
        SettingsChanged?.Invoke();
    }

    public void SetSfxVolume(float volume)
    {
        Settings.sfxVolume = Mathf.Clamp01(volume);

        ApplySfxVolume();
        SettingsChanged?.Invoke();
    }

    public void SetMusicMode(MusicMode mode)
    {
        if (Settings.musicMode == mode) return;

        Settings.musicMode = mode;
        SettingsChanged?.Invoke();
    }

    public void SetSelectedTrack(string trackName)
    {
        trackName ??= string.Empty;

        if (Settings.selectedTrack == trackName) return;

        Settings.selectedTrack = trackName;
        SettingsChanged?.Invoke();
    }

    public void SetLoopCurrentTrack(bool loop)
    {
        if (Settings.loopCurrentTrack == loop) return;

        Settings.loopCurrentTrack = loop;
        SettingsChanged?.Invoke();
    }

    public bool SaveSettings()
    {
        return SoundSettingsStore.Save(Settings);
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlaySfx(SfxId id)
    {
        if (
            sfxSource == null ||
            sfxLibrary == null
        )
        {
            return;
        }

        if (!sfxLibrary.TryGet(
                id,
                out AudioClip clip,
                out float volume
            ))
        {
            return;
        }

        sfxSource.PlayOneShot(
            clip,
            volume
        );
    }

    private void ApplyVolumes()
    {
        ApplyMusicVolume();
        ApplySfxVolume();
    }

    private void ApplyMusicVolume()
    {
        SetMixerVolume(MusicVolumeParameter, Settings.musicVolume);
    }

    private void ApplySfxVolume()
    {
        SetMixerVolume(SfxVolumeParameter, Settings.sfxVolume);
    }

    private void SetMixerVolume(string parameterName, float volume)
    {
        if (mixer == null) return;

        float decibels = volume <= 0f ? MinimumDecibels : Mathf.Log10(volume) * 20f;
        mixer.SetFloat(parameterName, decibels);
    }
}