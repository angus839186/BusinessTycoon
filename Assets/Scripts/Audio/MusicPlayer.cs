using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public sealed class MusicPlayer : MonoBehaviour
{
    private static readonly string[] SupportedExtensions =
    {
        ".ogg",
        ".mp3",
        ".wav"
    };

    [SerializeField]
    private AudioSystem audioSystem;

    private readonly List<string> trackPaths =
        new List<string>();

    private readonly List<string> trackNames =
        new List<string>();

    private AudioSource source;
    private AudioClip loadedClip;
    private Coroutine playRoutine;

    private string currentTrack =
        string.Empty;

    private bool isPaused;

    public IReadOnlyList<string> TrackNames =>
        trackNames;

    public string CurrentTrack =>
        currentTrack;

    public bool IsPaused =>
        isPaused;

    public bool IsPlaying =>
        source != null &&
        source.isPlaying;

    public string MusicFolderPath =>
        Path.Combine(
            Application.persistentDataPath,
            "Audio",
            "Music"
        );

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

    public void RefreshTracks()
    {
        trackPaths.Clear();
        trackNames.Clear();

        try
        {
            Directory.CreateDirectory(
                MusicFolderPath
            );

            string[] files =
                Directory.GetFiles(
                    MusicFolderPath,
                    "*",
                    SearchOption.TopDirectoryOnly
                );

            Array.Sort(
                files,
                StringComparer.OrdinalIgnoreCase
            );

            foreach (string filePath in files)
            {
                if (!IsSupported(filePath))
                {
                    continue;
                }

                trackPaths.Add(filePath);
                trackNames.Add(
                    Path.GetFileName(filePath)
                );
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"無法讀取音樂資料夾：" +
                $"{exception.Message}",
                this
            );
        }

        TracksChanged?.Invoke();
    }

    public void PlayConfiguredTrack()
    {
        if (trackPaths.Count == 0)
        {
            return;
        }

        SoundSettings settings =
            audioSystem.Settings;

        if (
            settings.musicMode ==
                MusicMode.Selected &&
            PlayTrack(settings.selectedTrack)
        )
        {
            return;
        }

        PlayNext();
    }

    public void PlaySelectedTrack()
    {
        if (audioSystem == null)
        {
            return;
        }

        if (!PlayTrack(
                audioSystem.Settings.selectedTrack
            ))
        {
            PlayNext();
        }
    }

    public bool PlayTrack(string trackName)
    {
        if (string.IsNullOrWhiteSpace(trackName))
        {
            return false;
        }

        for (int i = 0; i < trackNames.Count; i++)
        {
            if (!string.Equals(
                    trackNames[i],
                    trackName,
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                continue;
            }

            StartTrack(trackPaths[i]);
            return true;
        }

        return false;
    }

    public void PlayNext()
    {
        if (trackPaths.Count == 0)
        {
            return;
        }

        int index = GetNextTrackIndex();

        StartTrack(trackPaths[index]);
    }

    public void TogglePause()
    {
        if (source == null)
        {
            return;
        }

        if (source.clip == null)
        {
            PlayConfiguredTrack();
            return;
        }

        if (isPaused)
        {
            source.UnPause();
            isPaused = false;
        }
        else if (source.isPlaying)
        {
            source.Pause();
            isPaused = true;
        }

        PauseChanged?.Invoke(isPaused);
    }

    private void StartTrack(string filePath)
    {
        StopRoutine();

        playRoutine =
            StartCoroutine(
                LoadAndPlay(filePath)
            );
    }

    private IEnumerator LoadAndPlay(
        string filePath
    )
    {
        AudioType audioType =
            GetAudioType(filePath);

        string uri =
            new Uri(
                Path.GetFullPath(filePath)
            ).AbsoluteUri;

        using UnityWebRequest request =
            UnityWebRequestMultimedia.GetAudioClip(
                uri,
                audioType
            );

        DownloadHandlerAudioClip handler =
            request.downloadHandler
                as DownloadHandlerAudioClip;

        if (handler != null)
        {
            handler.streamAudio = true;
        }

        yield return request.SendWebRequest();

        if (
            request.result !=
            UnityWebRequest.Result.Success
        )
        {
            Debug.LogWarning(
                $"無法載入音樂 " +
                $"{Path.GetFileName(filePath)}：" +
                $"{request.error}",
                this
            );

            playRoutine = null;
            yield break;
        }

        AudioClip clip =
            DownloadHandlerAudioClip.GetContent(
                request
            );

        if (clip == null)
        {
            playRoutine = null;
            yield break;
        }

        ReleaseClip();

        loadedClip = clip;
        source.clip = loadedClip;

        currentTrack =
            Path.GetFileName(filePath);

        isPaused = false;

        source.loop =
            audioSystem.Settings.loopCurrentTrack;

        source.Play();

        TrackChanged?.Invoke(currentTrack);
        PauseChanged?.Invoke(false);

        WaitForSecondsRealtime wait =
            new WaitForSecondsRealtime(0.25f);

        while (
            source != null &&
            source.clip == loadedClip
        )
        {
            if (
                !isPaused &&
                !source.isPlaying
            )
            {
                break;
            }

            yield return wait;
        }

        if (
            source != null &&
            source.clip == loadedClip &&
            !isPaused
        )
        {
            playRoutine = null;
            PlayNext();
        }
    }

    private int GetNextTrackIndex()
    {
        if (trackPaths.Count == 1)
        {
            return 0;
        }

        int index;

        do
        {
            index = UnityEngine.Random.Range(
                0,
                trackPaths.Count
            );
        }
        while (
            string.Equals(
                trackNames[index],
                currentTrack,
                StringComparison.OrdinalIgnoreCase
            )
        );

        return index;
    }

    private void HandleSettingsChanged()
    {
        if (
            source == null ||
            audioSystem == null
        )
        {
            return;
        }

        source.loop =
            audioSystem.Settings.loopCurrentTrack;
    }

    private void StopRoutine()
    {
        if (playRoutine == null)
        {
            return;
        }

        StopCoroutine(playRoutine);
        playRoutine = null;
    }

    private void ReleaseClip()
    {
        if (source != null)
        {
            source.Stop();
            source.clip = null;
        }

        if (loadedClip != null)
        {
            Destroy(loadedClip);
            loadedClip = null;
        }
    }

    private static bool IsSupported(
        string filePath
    )
    {
        string extension =
            Path.GetExtension(filePath);

        foreach (
            string supportedExtension
            in SupportedExtensions
        )
        {
            if (string.Equals(
                    extension,
                    supportedExtension,
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                return true;
            }
        }

        return false;
    }

    private static AudioType GetAudioType(
        string filePath
    )
    {
        string extension =
            Path.GetExtension(filePath)
                .ToLowerInvariant();

        return extension switch
        {
            ".ogg" => AudioType.OGGVORBIS,
            ".mp3" => AudioType.MPEG,
            ".wav" => AudioType.WAV,
            _ => AudioType.UNKNOWN
        };
    }
}