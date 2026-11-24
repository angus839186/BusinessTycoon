using UnityEngine;

public sealed partial class AudioSystem
{
    private const string MusicVolumeParameter = "MusicVolume";
    private const string SfxVolumeParameter = "SfxVolume";
    private const float MinimumDecibels = -80f;

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