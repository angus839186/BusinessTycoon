using System;

[Serializable]
public sealed class SoundSettings
{
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    public MusicMode musicMode =
        MusicMode.Random;

    public string selectedTrack =
        string.Empty;

    public bool loopCurrentTrack;
}