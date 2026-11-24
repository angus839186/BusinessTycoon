using System;
using UnityEngine;

public sealed partial class MusicPlayer : MonoBehaviour
{

    [SerializeField]
    private AudioSystem audioSystem;

    public event Action TracksChanged;
    public event Action<string> TrackChanged;
    public event Action<bool> PauseChanged;

    private void Awake()
    {
        if (audioSystem == null)
        {
            audioSystem =
                GetComponent<AudioSystem>();
        }
    }

    private void Start()
    {
        if (audioSystem == null)
        {
            Debug.LogError(
                "MusicPlayer 找不到 AudioSystem。",
                this
            );

            return;
        }

        source = audioSystem.MusicSource;

        if (source == null)
        {
            Debug.LogError(
                "MusicPlayer 找不到 Music AudioSource。",
                this
            );

            return;
        }

        audioSystem.SettingsChanged +=
            HandleSettingsChanged;

        RefreshTracks();
        PlayConfiguredTrack();
    }

    private void OnDestroy()
    {
        if (audioSystem != null)
        {
            audioSystem.SettingsChanged -=
                HandleSettingsChanged;
        }

        StopRoutine();
        ReleaseClip();
    }


}