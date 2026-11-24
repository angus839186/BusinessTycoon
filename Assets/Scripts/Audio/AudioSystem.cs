using System;
using UnityEngine;
using UnityEngine.Audio;

public sealed partial class AudioSystem : MonoBehaviour
{


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

    private bool HasRequiredReferences =>
    mixer != null &&
    musicSource != null &&
    sfxSource != null &&
    sfxLibrary != null;

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
        if (!HasRequiredReferences)
        {
            Debug.LogError(
                "AudioSystem 缺少 AudioMixer、AudioSource 或 SfxLibrary 參考。",
                this
            );
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
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

}