using System;
using UnityEngine;

public sealed partial class MusicPlayer
{
    private AudioSource source;
    private string currentTrack = string.Empty;
    private bool isPaused;
    public string CurrentTrack => currentTrack;
    public bool IsPaused => isPaused;
    public bool IsPlaying => source != null && source.isPlaying;

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
}